using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Security;

namespace Identity.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IIdentityDbContext _dbContext;
    private readonly JwtTokenGenerator _tokenGenerator;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        IIdentityDbContext dbContext,
        JwtTokenGenerator tokenGenerator,
        JwtSettings jwtSettings,
        ILogger<AuthenticationService> logger)
    {
        _dbContext = dbContext;
        _tokenGenerator = tokenGenerator;
        _jwtSettings = jwtSettings;
        _logger = logger;
    }

    public async Task<LoginResponse?> AuthenticateAsync(string email, string password)
    {
        var userCredential = await _dbContext.UserCredentials
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

        if (userCredential == null)
        {
            _logger.LogWarning("User not found for email: {Email}", email);
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(password, userCredential.PasswordHash))
        {
            _logger.LogWarning("Invalid password for email: {Email}", email);
            return null;
        }

        userCredential.LastLoginAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        var token = _tokenGenerator.GenerateToken(userCredential.CustomerId, userCredential.Email, userCredential.Role);
        
        _logger.LogInformation("Token generated for customer: {CustomerId}", userCredential.CustomerId);

        return new LoginResponse
        {
            Token = token,
            CustomerId = userCredential.CustomerId,
            Email = userCredential.Email,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes)
        };
    }
}
