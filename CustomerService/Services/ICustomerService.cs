using Shared.Models;

namespace CustomerService.Services
{
    public interface ICustomerService
    {
        Task<Customer> GetCustomerAsync(int id);
    }
}
