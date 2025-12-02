using Identity.API.Models;

namespace Identity.API.Services;

public interface IAuthenticationService
{
    Task<LoginResponse?> AuthenticateAsync(string email, string password);
}
