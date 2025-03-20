using OrderService.Data;
using Shared.Models;
using Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using OrderService.Messaging;
using System.Collections.Concurrent;
using OrderService.Extensions;
using Microsoft.AspNetCore.Http;
using RabbitMQ.Client.Events;
using System.Text.Json;
using RabbitMQ.Client; // Required for IHttpContextAccessor

namespace OrderService.Services
{
    public class OrderService : IOrderService
    {
        private readonly OrderContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OrderService> _logger;
        private readonly IRabbitMQProducer _rabbitMQProducer;
        private readonly IHttpContextAccessor _httpContextAccessor; // Inject IHttpContextAccessor
        private readonly ConcurrentDictionary<string, TaskCompletionSource<ProductVerificationResponse>> _pendingRequests = new();

        public OrderService(OrderContext context, IHttpClientFactory httpClientFactory, ILogger<OrderService> logger, IRabbitMQProducer rabbitMQProducer, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _rabbitMQProducer = rabbitMQProducer;
            _httpContextAccessor = httpContextAccessor;
            var consumer = new EventingBasicConsumer(_rabbitMQProducer.Channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var response = JsonSerializer.Deserialize<ProductVerificationResponse>(body);
                if (response != null && _pendingRequests.TryRemove(ea.BasicProperties.CorrelationId, out var tcs))
                {
                    tcs.SetResult(response);
                }
            };
            _rabbitMQProducer.Channel.BasicConsume(queue: "product_verification_queue",
                                           autoAck: true,
                                           consumer: consumer);
            _rabbitMQProducer.Channel.BasicConsume(queue: "product_verification_response_queue",
                                           autoAck: true,
                                           consumer: consumer);
        }

        public async Task<Order> CreateOrderAsync(CreateOrderDto orderDto)
        {
            _logger.LogInformation($"Validating customer existence. CustomerId: {orderDto.CustomerId}");

            var client = _httpClientFactory.CreateClient();
            var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorizationHeader.Substring("Bearer ".Length).Trim());
            }
            else
            {
                _logger.LogWarning("No Authorization header found in request to OrderService.");
                throw new ArgumentException("Missing Authorization header.");
            }
            var customerServiceUrl = $"http://customer_service/api/customers/{orderDto.CustomerId}";
            var customerResponse = await client.GetAsync(customerServiceUrl);

            if (!customerResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Customer validation failed. CustomerId: {orderDto.CustomerId}, StatusCode: {customerResponse.StatusCode}");
                throw new ArgumentException("Customer does not exist.");
            }

            _logger.LogInformation($"Customer validation succeeded. CustomerId: {orderDto.CustomerId}");

            if (orderDto.Quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than zero.");
            }

            var verificationRequest = new ProductVerificationRequest
            {
                ProductId = orderDto.ProductId,
                Quantity = orderDto.Quantity,
            };
            _logger.LogInformation($"Publishing Product Verification Request: ProductId={verificationRequest.ProductId}, Quantity={verificationRequest.Quantity}");

            var correlatinId = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<ProductVerificationResponse>();
            _pendingRequests[correlatinId] = tcs;

            _rabbitMQProducer.PublishMessage("product_verification_queue", verificationRequest, correlatinId);
            _logger.LogInformation($"Sent product verification request. ProductId: {orderDto.ProductId}, Quantity: {orderDto.Quantity}");
            ProductVerificationResponse verificationResponse;
            
            var order = new Order
            {
                CustomerId = orderDto.CustomerId,
                ProductId = orderDto.ProductId,
                Quantity = orderDto.Quantity,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 0
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return order;
        }
        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _context.Orders.FindAsync(id);
        }

        public async Task<bool> UpdateOrderAsync(Order order)
        {
            var existingOrder = await _context.Orders.FindAsync(order.Id);
            if (existingOrder == null)
            {
                return false;
            }

            existingOrder.Quantity = order.Quantity;
            existingOrder.TotalAmount = order.TotalAmount;

            _context.Entry(existingOrder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Orders.Any(o => o.Id == order.Id))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return false;
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}