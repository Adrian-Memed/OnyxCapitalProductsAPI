using FluentValidation;
using FluentValidation.Results;
using ProductsWebAPI.Interfaces;
using ProductsWebAPI.Models;

namespace ProductsWebAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IValidator<Product> _productValidator;
        private readonly IValidator<string> _colourValidator;
        public ProductService(IProductRepository productRepository, IValidator<Product> productValidator, IValidator<string> colourValidator)
        {
            _productRepository = productRepository;
            _productValidator = productValidator;
            _colourValidator = colourValidator;
        }
        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _productRepository.GetAllProductsAsync();
        }
        public async Task<IEnumerable<Product>> GetProductsByColourAsync(string colour)
        {
            colour = CapitalizeFirstLetter(colour);

            ValidationResult validationResult = await _colourValidator.ValidateAsync(colour); 
            
            return await _productRepository.GetProductsByColourAsync(colour);
        }
        public async Task<Product> AddProductAsync(CreateProductDto productDto)
        {
            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Colour = productDto.Colour,
                Price = productDto.Price
            };

            product.Colour = CapitalizeFirstLetter(product.Colour);

            ValidationResult validationResult = await _productValidator.ValidateAsync(product);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            int id = await _productRepository.AddProductAsync(product);
            
            product.Id = id;

            return product;
        }

        public string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            
            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }

        
    }
}
