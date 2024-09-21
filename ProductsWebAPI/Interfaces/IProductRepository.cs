using ProductsWebAPI.Models;

namespace ProductsWebAPI.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<IEnumerable<Product>> GetProductsByColourAsync(string colour);
        Task<int> AddProductAsync(Product product);
    }
}
