using ProductsWebAPI.Models;

namespace ProductsWebAPI.Interfaces
{
    public interface IAuthenticationService
    {
        string? Authenticate(LoginModel model);
    }
}
