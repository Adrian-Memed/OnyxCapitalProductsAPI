using Dapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Data.Sqlite;
using Moq;
using ProductsWebAPI.Helper;
using ProductsWebAPI.Models;
using ProductsWebAPI.Repository;
using ProductsWebAPI.Services;
using System.Data;


namespace ProductsWebAPI.Tests
{
    public class ProductServiceIntegrationTests : IAsyncLifetime
    {
        private readonly IDbConnection _dbConnection;
        private readonly ProductRepository _productRepository;
        private readonly ProductService _productService;

        public ProductServiceIntegrationTests()
        {
            // Set up SQLite in-memory database
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            _dbConnection = connection;
            var dapperHelper = new DapperHelper();  
            _productRepository = new ProductRepository(_dbConnection, dapperHelper);

            var productValidator = new Mock<IValidator<Product>>();
            var colourValidator = new Mock<IValidator<string>>();
            productValidator
                .Setup(v => v.ValidateAsync(It.IsAny<Product>(), default))
                .ReturnsAsync(new ValidationResult());  // No validation errors

            colourValidator
                .Setup(v => v.ValidateAsync(It.IsAny<string>(), default))
                .ReturnsAsync(new ValidationResult());  // No validation errors

            _productService = new ProductService(_productRepository, productValidator.Object, colourValidator.Object);

            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            var createTableQuery = @"
            CREATE TABLE Products (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT,
                Colour TEXT NOT NULL,
                Price REAL NOT NULL
            );";
            _dbConnection.Execute(createTableQuery);
        }

        [Fact]
        public async Task AddProduct_ShouldInsertProductIntoDatabase()
        {
            // Arrange: Create a new product DTO
            var newProduct = new CreateProductDto { Name = "Test Product", Description = "Test Description", Colour = "Red", Price = 12.99M };

            // Act: Add the product via the service
            var addedProduct = await _productService.AddProductAsync(newProduct);

            // Assert: Check that the product was added to the database
            var productsInDb = await _productService.GetAllProductsAsync();
            Assert.NotNull(productsInDb);
            Assert.Single(productsInDb);
            Assert.Equal("Test Product", addedProduct.Name);
            Assert.Equal("Red", addedProduct.Colour);
            Assert.Equal(12.99M, addedProduct.Price);
        }

        [Fact]
        public async Task GetProductsByColour_ShouldReturnFilteredProducts()
        {
            // Arrange: Insert two products into the database
            var redProduct = new CreateProductDto { Name = "Red Product", Description = "Test Description", Colour = "Red", Price = 12.99M };
            var blueProduct = new CreateProductDto { Name = "Blue Product", Description = "Test Description", Colour = "Blue", Price = 15.99M };

            await _productService.AddProductAsync(redProduct);
            await _productService.AddProductAsync(blueProduct);

            // Act: Retrieve products by colour via the service
            var redProducts = await _productService.GetProductsByColourAsync("red");

            // Assert: Only the red product should be returned
            Assert.NotNull(redProducts);
            Assert.Single(redProducts);
            Assert.Equal("Red", redProducts.First().Colour);
            Assert.Equal("Red Product", redProducts.First().Name);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public Task DisposeAsync()
        {
            _dbConnection.Dispose();
            return Task.CompletedTask;
        }
    }
}
