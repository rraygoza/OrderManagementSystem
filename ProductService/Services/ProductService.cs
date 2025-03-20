using ProductService.Data;
using Shared.Models;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs;
using ProductService.Messaging;

namespace ProductService.Services
{
    public class ProductService : IProductService
    {
        private readonly ProductContext _context;
        private readonly ILogger<ProductService> _logger;
        private readonly IRabbitMQProducer _rabbitMQProducer;
        public ProductService(ProductContext context, ILogger<ProductService> logger, IRabbitMQProducer rabbitMQProducer)
        {
            _context = context;
            _logger = logger;
            _rabbitMQProducer = rabbitMQProducer;
        }
        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<ProductVerificationResponse> VerifyProductAsync(ProductVerificationRequest request)
        {
            _logger.LogInformation($"VerifyProductAsync called: ProductId={request.ProductId}");

            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null)
            {
                _logger.LogWarning($"Product verification failed. Product ID {request.ProductId} not found.");
                var responseNotFound = new ProductVerificationResponse {ProductId = request.ProductId, IsValid = false, Message = "Product not found." };
                _logger.LogInformation($"Publishing ProductVerificationResponse: IsValid={responseNotFound.IsValid}, Message= {responseNotFound.Message}");
                _rabbitMQProducer.PublishMessage("product_verification_response_queue", responseNotFound);
                return responseNotFound;
            }

            if (product.Stock < request.Quantity)
            {
                _logger.LogWarning($"Product verification failed. Insufficient stock for Product ID {request.ProductId}.");
                var responseInsufficient = new ProductVerificationResponse {ProductId = request.ProductId, IsValid = false, Message = "Insufficient stock."};
                _rabbitMQProducer.PublishMessage("product_verification_response_queue", responseInsufficient);
                return responseInsufficient;
            }

            _logger.LogInformation($"Product verification successful. Product ID {request.ProductId}.");
            var response = new ProductVerificationResponse
            {
                ProductId = request.ProductId,
                IsValid = true,
                Price = product.Price,
            };

            _logger.LogInformation($"Publishing ProductVerificationResponse: IsValid={response.IsValid}, Price={response.Price}");
            _rabbitMQProducer.PublishMessage("product_verification_response_queue", response);
            return response;
        }


    }
}
