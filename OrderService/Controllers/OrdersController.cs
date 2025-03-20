using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Data;
using OrderService.Messaging;
using OrderService.Services;
using Shared.DTOs;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Json;
using Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly OrderContext _context;
        private readonly IOrderService _orderService;
        private readonly IRabbitMQProducer _rabbitMQProducer;
        private readonly ILogger<OrdersController> _logger;
        public OrdersController(OrderContext orderContext, IOrderService orderService, IRabbitMQProducer rabbitMQProducer, ILogger<OrdersController> logger)
        {
            _logger = logger;
            _context = orderContext;
            _orderService = orderService;
            _rabbitMQProducer = rabbitMQProducer;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                _logger.LogWarning($"Order with id {id} not found.");
                return NotFound();
            }
            return Ok(order);
        }
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();
            var orders = _context.Orders.ToList();
            if (orders.Count ==0)
            {
                _logger.LogWarning($"There are no orders");
                return NotFound();
            }
            return Ok(orders);
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrderAsync([FromBody]CreateOrderDto orderDto)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();

            _logger.LogInformation("Creating order for customer {customerId}, product {productId} and for this quantity {quantity}", orderDto.CustomerId, orderDto.ProductId, orderDto.Quantity);
            if(!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid order data");
                return BadRequest(ModelState);
            }

            try
            {
                var order = await _orderService.CreateOrderAsync(orderDto);
                var logDto = new CreatedLogDto
                {
                    OrderId = order.Id,
                    TimeStamp = DateTime.UtcNow,
                    Message = $"Order {order.Id} created by user {userId}."
                };
                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"CreateOrder request failed: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "An unexpected error occurred while creating the order.");
                return StatusCode(500, "Internal Server Error");
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] Order order)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized();
            if (id != order.Id)
            {
                _logger.LogWarning($"Order id {id} does not match the id in the request body.");
                return BadRequest();
            }
            
            var existingOrder = await _context.Orders.FindAsync(id);
            if (existingOrder == null)
            {
                _logger.LogWarning($"Order with id {id} not found.");
                return NotFound();
            }
            existingOrder.Quantity = order.Quantity;
            existingOrder.TotalAmount = order.TotalAmount;
            _context.Entry(existingOrder).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!_context.Orders.Any(o => o.Id == id))
                {
                    _logger.LogWarning($"Order with ID {id} not found during update (concurrency).");
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized();
            }
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private string GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim?.Value;
        }

    }
}
