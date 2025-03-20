using Shared.DTOs;
using Shared.Models;

namespace ProductService.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product> GetProductByIdAsync(int id);
        Task<ProductVerificationResponse> VerifyProductAsync(ProductVerificationRequest request);
    }
}
