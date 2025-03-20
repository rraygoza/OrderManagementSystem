using CustomerService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomersController> _logger;
        public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching customer with ID: {id}");
                var customer = await _customerService.GetCustomerAsync(id);
                if(customer== null)
                {
                    _logger.LogWarning($"Customer with ID: {id} not found.");
                    return NotFound("Customer not found.");
                }
                return Ok(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the customer.");
                return StatusCode(500, "An error occurred while fetching the customer.");
            }
        }

    }
}
