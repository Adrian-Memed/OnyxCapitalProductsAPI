using Microsoft.AspNetCore.Mvc;
using ProductsWebAPI.Interfaces;
using ProductsWebAPI.Models;

namespace ProductsWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        public AuthController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            var token = _authenticationService.Authenticate(model);

            if (token is null)
            {
                return Unauthorized();
            }
            return Ok(new { Token = token });
        }
    }
}
