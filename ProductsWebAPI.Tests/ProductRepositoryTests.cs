using Moq;
using ProductsWebAPI.Interfaces;
using ProductsWebAPI.Models;
using ProductsWebAPI.Repository;
using System.Data;

namespace ProductsWebAPI.Tests
{
    public class ProductRepositoryTests
    {
        private readonly Mock<IDbConnection> _mockDbConnection;
        private readonly Mock<IDapperHelper> _mockDapperHelper;
        private readonly ProductRepository _repository;

        public ProductRepositoryTests()
        {
            _mockDbConnection = new Mock<IDbConnection>();
            _mockDapperHelper = new Mock<IDapperHelper>();
            _repository = new ProductRepository(_mockDbConnection.Object, _mockDapperHelper.Object);
        }

        [Fact]
        public async Task GetAllProductsAsync_ShouldReturnProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Product1", Colour = "Red", Price = 10.99M },
                new Product { Id = 2, Name = "Product2", Colour = "Blue", Price = 15.99M }
            };

            // Mock Dapper's QueryAsync method
            _mockDapperHelper
                .Setup(helper => helper.QueryAsync<Product>(_mockDbConnection.Object, It.IsAny<string>(), null))
                .ReturnsAsync(products);

            // Act
            var result = await _repository.GetAllProductsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("Product1", result.First().Name);

            // Verify that the connection's QueryAsync method was called
            _mockDapperHelper.Verify(helper => helper.QueryAsync<Product>(_mockDbConnection.Object, It.IsAny<string>(), null), Times.Once);
        }

        [Fact]
        public async Task GetProductsByColourAsync_ShouldReturnFilteredProducts()
        {
            // Arrange
            string colour = "Red";
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Product1", Colour = "Red", Price = 10.99M },
                new Product{ Id = 2, Name = "Product2", Colour = "Yellow", Price = 14.65M }
            };

            //only return the product that matches the colour "Red"
            var filteredProducts = products.Where(x => x.Colour == colour).ToList();

            _mockDapperHelper
                .Setup(helper => helper.QueryAsync<Product>(_mockDbConnection.Object, It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(filteredProducts);

            // Act
            var result = await _repository.GetProductsByColourAsync(colour);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(colour, result.First().Colour);

            // Verify that QueryAsync was called with the correct parameters
            
            _mockDapperHelper.Verify(helper => helper
                .QueryAsync<Product>(
                _mockDbConnection.Object, 
                It.IsAny<string>(), 
                It.Is<object>(param => 
                    param.GetType().GetProperty("Colour").GetValue(param).ToString() == colour
                )
            ), Times.Once);
        }

        [Fact]
        public async Task AddProductAsync_ShouldReturnNewProductId()
        {
            // Arrange
            var product = new Product { Name = "New Product", Colour = "Green", Price = 25.00M };

            _mockDapperHelper
                .Setup(helper => helper.ExecuteScalarAsync<int>(_mockDbConnection.Object, It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(1);

            // Act
            int result = await _repository.AddProductAsync(product);

            // Assert
            Assert.Equal(1, result);

            // Verify that ExecuteScalarAsync was called once
            _mockDapperHelper.Verify(helper => helper.ExecuteScalarAsync<int>(_mockDbConnection.Object, It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }
    }
}
