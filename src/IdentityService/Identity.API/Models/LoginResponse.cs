namespace Identity.API.Models;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
