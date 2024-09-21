using Dapper;
using Microsoft.Data.Sqlite;
using ProductsWebAPI.Helper;
using ProductsWebAPI.Interfaces;
using ProductsWebAPI.Models;
using ProductsWebAPI.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductsWebAPI.Tests
{
    public class ProductRepositoryIntegrationTests : IAsyncLifetime
    {
        private readonly IDbConnection _dbConnection;
        private readonly DapperHelper _dapperHelper;
        private readonly ProductRepository _productRepository;
        public ProductRepositoryIntegrationTests() 
        {
            //set up in-memory database
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();  

            _dbConnection = connection;
            _dapperHelper = new DapperHelper();

            _productRepository = new ProductRepository(_dbConnection, _dapperHelper);

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
                Price DECIMAL NOT NULL
            );";

            _dbConnection.Execute(createTableQuery);
        }

        [Fact]
        public async Task GetAllProducts_ShouldReturnEmptyInitially()
        {
            // Act: Call the repository method
            IEnumerable<Product> products = await _productRepository.GetAllProductsAsync();

            // Assert: Check that the result is empty initially
            Assert.NotNull(products);
            Assert.Empty(products);
        }

        [Fact]
        public async Task AddProduct_ShouldInsertProductIntoDatabase()
        {
            // Arrange: Create a new product
            var newProduct = new Product { Name = "Test Product", Description = "Large", Colour = "Yellow", Price = 25.99M };

            // Act: Insert the product into the in-memory database
            int newProductId = await _productRepository.AddProductAsync(newProduct);

            // Assert: Verify the product was inserted and can be retrieved
            var insertedProduct = await _productRepository.GetProductsByColourAsync(newProduct.Colour);
            Assert.NotNull(insertedProduct);
            Assert.Equal("Test Product", insertedProduct.First().Name);
            Assert.Equal("Yellow", insertedProduct.First().Colour);
            Assert.Equal(25.99M, insertedProduct.First().Price);
        }
        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            _dbConnection.Dispose();
            return Task.CompletedTask;
        }
    }
}
