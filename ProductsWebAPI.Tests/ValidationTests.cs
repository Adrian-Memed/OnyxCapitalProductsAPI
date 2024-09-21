using FluentValidation.TestHelper;
using ProductsWebAPI.Models;
using ProductsWebAPI.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductsWebAPI.Tests
{
    public class ValidationTests
    {
        private readonly ProductValidator _productValidator;

        public ValidationTests()
        {
            _productValidator = new ProductValidator();
        }

        [Fact]
        public void ProductValidator_ShouldPass_ForValidProduct()
        {
            // Arrange
            var validProduct = new Product
            {
                Name = "Valid Product",
                Colour = "Red",
                Price = 25.99M
            };

            // Act
            var result = _productValidator.TestValidate(validProduct);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void ProductValidator_ShouldFail_WhenNameIsEmpty()
        {
            // Arrange
            var invalidProduct = new Product
            {
                Name = "",  
                Colour = "Red",
                Price = 25.99M
            };

            // Act
            var result = _productValidator.TestValidate(invalidProduct);

            // Assert
            result.ShouldHaveValidationErrorFor(product => product.Name)
                  .WithErrorMessage("Product name is required.");
        }

        [Fact]
        public void ProductValidator_ShouldFail_WhenNameIsTooShort()
        {
            // Arrange
            var invalidProduct = new Product
            {
                Name = "AB",   
                Colour = "Red",
                Price = 25.99M
            };

            // Act
            var result = _productValidator.TestValidate(invalidProduct);

            // Assert
            result.ShouldHaveValidationErrorFor(product => product.Name)
                  .WithErrorMessage("Product name must be at least 3 characters long.");
        }

        [Fact]
        public void ProductValidator_ShouldFail_WhenPriceIsLessThanZero()
        {
            // Arrange
            var invalidProduct = new Product
            {
                Name = "Valid Product",
                Colour = "Red",
                Price = -5M   
            };

            // Act
            var result = _productValidator.TestValidate(invalidProduct);

            // Assert
            result.ShouldHaveValidationErrorFor(product => product.Price)
                  .WithErrorMessage("Price must be greater than 0.");
        }

        [Fact]
        public void ProductValidator_ShouldFail_WhenInvalidColour()
        {
            // Arrange
            var invalidProduct = new Product
            {
                Name = "Valid Product",
                Colour = "InvalidColour",   
                Price = 25.99M
            };

            // Act
            var result = _productValidator.TestValidate(invalidProduct);

            // Assert
            result.ShouldHaveValidationErrorFor(product => product.Colour)
                  .WithErrorMessage($"'{invalidProduct.Colour}' is not a valid colour."); 
        }
    }
}

