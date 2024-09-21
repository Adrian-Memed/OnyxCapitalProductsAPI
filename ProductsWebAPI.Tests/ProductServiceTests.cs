using FluentValidation;
using FluentValidation.Results;
using Moq;
using ProductsWebAPI.Interfaces;
using ProductsWebAPI.Models;
using ProductsWebAPI.Services;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValidationException = FluentValidation.ValidationException;
using ValidationResult = FluentValidation.Results.ValidationResult;


namespace ProductsWebAPI.Tests
{
    public class ProductServiceTests
    {
        private readonly Mock<IValidator<string>> _mockColourValidator;
        private readonly Mock<IValidator<Product>> _mockProductValidator;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _mockColourValidator = new Mock<IValidator<string>>();
            _mockProductValidator = new Mock<IValidator<Product>>();
            _mockProductRepository = new Mock<IProductRepository>();
            _productService = new ProductService(_mockProductRepository.Object, _mockProductValidator.Object, _mockColourValidator.Object);
        }

        [Fact]
        public async Task GetAllProductsAsync_ShouldReturnProducts()
        {
            // Arrange: Mock repository to return a list of products
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Product1", Colour = "Red", Price = 10.99M },
                new Product { Id = 2, Name = "Product2", Colour = "Blue", Price = 15.99M }
            };

            _mockProductRepository.Setup(repo => repo.GetAllProductsAsync()).ReturnsAsync(products);

            // Act: Call the service
            var result = await _productService.GetAllProductsAsync();

            // Assert: Verify the result
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());

            Assert.Equal("Product1", result.First().Name);
        }

        [Fact]
        public async Task AddProductAsync_ShouldReturnProduct_WhenValidationPasses()
        {
            // Arrange
            var productDto = new CreateProductDto
            {
                Name = "Test Product",
                Description = "Test Description",
                Colour = "Red",
                Price = 20.00M
            };

            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Colour = productDto.Colour,
                Price = productDto.Price
            };

            // Set up the validator to return a valid result
            _mockProductValidator
                .Setup(v => v.ValidateAsync(It.IsAny<Product>(), default))
                .ReturnsAsync(new ValidationResult()); // Valid result (no errors)

            _mockProductRepository
                .Setup(repo => repo.AddProductAsync(It.IsAny<Product>()))
                .ReturnsAsync(1); // Return the new product ID

            // Act
            var result = await _productService.AddProductAsync(productDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Product", result.Name);
            _mockProductValidator.Verify(v => v.ValidateAsync(It.IsAny<Product>(), default), Times.Once);
        }

        [Fact]
        public async Task AddProductAsync_ShouldThrowValidationException_WhenValidationFails()
        {
            // Arrange
            var productDto = new CreateProductDto
            {
                Name = "Invalid Product",
                Description = "Invalid Description",
                Colour = "Red",
                Price = -1 // Invalid price
            };

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Price", "Price must be greater than zero")
            };

            // Set up the validator to return a failed result
            _mockProductValidator
                .Setup(v => v.ValidateAsync(It.IsAny<Product>(), default))
                .ReturnsAsync(new ValidationResult(validationFailures)); // Invalid result

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ValidationException>(() => _productService.AddProductAsync(productDto));
            Assert.Equal("Price must be greater than zero", ex.Errors.First().ErrorMessage);
            _mockProductValidator.Verify(v => v.ValidateAsync(It.IsAny<Product>(), default), Times.Once);
        }

        [Fact]
        public async Task GetProductsByColourAsync_ShouldReturnProducts_WhenColourIsValid()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Product1", Colour = "Red", Price = 10.99M }
            };

            // Set up the colour validator to return a valid result
            _mockColourValidator
                .Setup(v => v.ValidateAsync(It.IsAny<string>(), default))
                .ReturnsAsync(new ValidationResult()); // Valid result (no errors)

            // Set up the repository to return products
            _mockProductRepository
                .Setup(repo => repo.GetProductsByColourAsync(It.IsAny<string>()))
                .ReturnsAsync(products);

            // Act
            var result = await _productService.GetProductsByColourAsync("red");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Red", result.First().Colour);
            _mockColourValidator.Verify(v => v.ValidateAsync(It.IsAny<string>(), default), Times.Once);
        }

        [Theory]
        [InlineData("red", "Red")]
        [InlineData("YELLOW", "Yellow")]
        [InlineData("bLuE", "Blue")]
        [InlineData("", "")]
        [InlineData(null, null)]
        public void CapitalizeFirstLetter_ShouldCapitalizeCorrectly(string input, string expected)
        {
            // Act
            var result = _productService.CapitalizeFirstLetter(input);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}