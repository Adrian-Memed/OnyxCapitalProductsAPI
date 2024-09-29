using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using ProductsWebAPI.Interfaces;
using ProductsWebAPI.Models;
using System.Threading.RateLimiting;

namespace ProductsWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly Services.RateLimiting.SlidingWindowRateLimiter _rateLimiter;

        public ProductsController(IProductService productService, Services.RateLimiting.SlidingWindowRateLimiter rateLimiter)
        {
            _productService = productService;
            _rateLimiter = rateLimiter;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            string? userId = HttpContext.User.GetDisplayName();

            if (await _rateLimiter.IsRateLimitedAsync(userId))
            {
                return StatusCode(429, "Too many requests. Please try again later.");
            }

            IEnumerable<Product> products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        [Authorize]
        [HttpGet("{colour}")]
        public async Task<IActionResult> GetProductsByColour(string colour)
        {
            string? userId = HttpContext.User.GetDisplayName();

            if (await _rateLimiter.IsRateLimitedAsync(userId))
            {
                return StatusCode(429, "Too many requests. Please try again later.");
            }

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
            string? userId = HttpContext.User.GetDisplayName();

            if (await _rateLimiter.IsRateLimitedAsync(userId))
            {
                return StatusCode(429, "Too many requests. Please try again later.");
            }

            var product = await _productService.AddProductAsync(dto);
            return StatusCode(201, product);
        }

    }
}
