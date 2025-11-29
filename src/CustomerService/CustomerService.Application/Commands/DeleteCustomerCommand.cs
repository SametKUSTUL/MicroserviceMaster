using MediatR;

namespace CustomerService.Application.Commands;

public record DeleteCustomerCommand(Guid Id) : IRequest<bool>;
