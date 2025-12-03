namespace Identity.Application.Services;

public interface IAuthenticationService
{
    Task<LoginResponse?> AuthenticateAsync(string email, string password);
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
