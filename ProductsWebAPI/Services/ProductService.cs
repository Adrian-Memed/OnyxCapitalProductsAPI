using FluentValidation;
using FluentValidation.Results;
using ProductsWebAPI.Interfaces;
using ProductsWebAPI.Models;
using ProductsWebAPI.Services.Caching;

namespace ProductsWebAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IValidator<Product> _productValidator;
        private readonly IValidator<string> _colourValidator;
        private readonly IRedisCacheService _cache;

        public ProductService(IProductRepository productRepository,
                              IValidator<Product> productValidator,
                              IValidator<string> colourValidator,
                              IRedisCacheService cache)
        {
            _productRepository = productRepository;
            _productValidator = productValidator;
            _colourValidator = colourValidator;
            _cache = cache;
        }
        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            IEnumerable<Product>? products = await _cache.GetData<IEnumerable<Product>>(nameof(Product).ToLower());
            if (products is not null)
            {
                return products;
            }
            products = await _productRepository.GetAllProductsAsync();

            await _cache.SetData(nameof(Product).ToLower(), products);
            return products;
        }
        public async Task<IEnumerable<Product>> GetProductsByColourAsync(string colour)
        {
            colour = CapitalizeFirstLetter(colour);

            ValidationResult validationResult = await _colourValidator.ValidateAsync(colour);

            var cacheKey = $"products_by_colour_{colour}";
            var cachedProducts = await _cache.GetData<IEnumerable<Product>>(cacheKey);
            if (cachedProducts != null)
            {
                return cachedProducts;
            }

            IEnumerable<Product> productsByColour = await _productRepository.GetProductsByColourAsync(colour);
            if (productsByColour.Any())
            {
                await _cache.SetData(cacheKey, productsByColour);
            }
            return productsByColour;
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
