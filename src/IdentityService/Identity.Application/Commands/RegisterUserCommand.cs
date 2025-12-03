using MediatR;

namespace Identity.Application.Commands;

public record RegisterUserCommand(string Email, string Password) : IRequest<RegisterUserResult>;

public record RegisterUserResult(string Email, string CustomerId, string Message);
