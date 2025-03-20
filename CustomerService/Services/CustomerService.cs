
using CustomerService.Data;
using Shared.Models;

namespace CustomerService.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly CustomerContext _context;
        private readonly ILogger<CustomerService> _logger;
        public CustomerService(CustomerContext context, ILogger<CustomerService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<Customer> GetCustomerAsync(int id)
        {
            _logger.LogInformation($"Fetching customer with ID: {id}");
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                _logger.LogWarning($"Customer with ID: {id} not found.");
                throw new ArgumentException("Customer not found.");
            }
            return customer;
        }
    }
}
