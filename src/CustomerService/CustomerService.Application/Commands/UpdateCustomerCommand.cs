using CustomerService.Domain.Entities;
using MediatR;

namespace CustomerService.Application.Commands;

public record UpdateCustomerCommand(
    Guid Id,
    string Name,
    string Surname,
    string Email,
    string Phone,
    CustomerStatus Status
) : IRequest<Customer>;
