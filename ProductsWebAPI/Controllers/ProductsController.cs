using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductsWebAPI.Interfaces;
using ProductsWebAPI.Models;

namespace ProductsWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            IEnumerable<Product> products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        [Authorize]
        [HttpGet("{colour}")]
        public async Task<IActionResult> GetProductsByColour(string colour)
        {
            IEnumerable<Product> product = await _productService.GetProductsByColourAsync(colour);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
        {
            var product = await _productService.AddProductAsync(dto);
            return StatusCode(201, product);
        }
    }
}
