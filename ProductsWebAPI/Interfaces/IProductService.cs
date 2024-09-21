using ProductsWebAPI.Models;

namespace ProductsWebAPI.Interfaces
{
    public interface IProductService
    {
        Task<Product> AddProductAsync(CreateProductDto productDto);
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<IEnumerable<Product>> GetProductsByColourAsync(string colour);
    }
}