using Shared.DTOs;
using Shared.Models;

namespace OrderService.Services
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(CreateOrderDto orderDto);
    }
}
