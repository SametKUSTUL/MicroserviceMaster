using Identity.Application.Commands;
using Identity.Application.Events;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Handlers;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IIdentityDbContext _dbContext;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<RegisterUserHandler> _logger;

    public RegisterUserHandler(
        IIdentityDbContext dbContext,
        IMessagePublisher messagePublisher,
        ILogger<RegisterUserHandler> logger)
    {
        _dbContext = dbContext;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _dbContext.UserCredentials
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException("User already exists");
        }

        var customerId = $"CUST{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";

        var userCredential = new UserCredential
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "Customer",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.UserCredentials.Add(userCredential);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var userRegisteredEvent = new UserRegisteredEvent
        {
            Email = request.Email,
            CustomerId = customerId,
            RegisteredAt = DateTime.UtcNow
        };

        _messagePublisher.Publish(userRegisteredEvent, "user.registered");

        _logger.LogInformation("User registered: {Email}, CustomerId: {CustomerId}", request.Email, customerId);

        return new RegisterUserResult(request.Email, customerId, "User registered successfully");
    }
}
