using Identity.API.Models;
using Security;

namespace Identity.API.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly JwtTokenGenerator _tokenGenerator;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        JwtTokenGenerator tokenGenerator, 
        JwtSettings jwtSettings,
        ILogger<AuthenticationService> logger)
    {
        _tokenGenerator = tokenGenerator;
        _jwtSettings = jwtSettings;
        _logger = logger;
    }

    public async Task<LoginResponse?> AuthenticateAsync(string email, string password)
    {
        // Demo amaçlı basit kullanıcı kontrolü
        // Gerçek uygulamada database'den kullanıcı kontrolü yapılmalı
        var user = await ValidateUserCredentials(email, password);
        
        if (user == null)
        {
            _logger.LogWarning("Authentication failed for email: {Email}", email);
            return null;
        }

        var token = _tokenGenerator.GenerateToken(user.CustomerId, user.Email, user.Role);
        
        _logger.LogInformation("Token generated for customer: {CustomerId}", user.CustomerId);

        return new LoginResponse
        {
            Token = token,
            CustomerId = user.CustomerId,
            Email = user.Email,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes)
        };
    }

    private Task<UserInfo?> ValidateUserCredentials(string email, string password)
    {
        // Demo kullanıcılar - Gerçek uygulamada database'den gelecek
        var demoUsers = new List<UserInfo>
        {
            new() { CustomerId = "1", Email = "customer1@test.com", Password = "password123", Role = "Customer" },
            new() { CustomerId = "2", Email = "customer2@test.com", Password = "password123", Role = "Customer" },
            new() { CustomerId = "admin", Email = "admin@test.com", Password = "admin123", Role = "Admin" }
        };

        var user = demoUsers.FirstOrDefault(u => 
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && 
            u.Password == password);

        return Task.FromResult(user);
    }

    private class UserInfo
    {
        public string CustomerId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
