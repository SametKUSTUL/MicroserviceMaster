using CustomerService.Domain.Entities;
using MediatR;

namespace CustomerService.Application.Commands;

public record CreateCustomerCommand(
    string CustomerId,
    string Name,
    string Surname,
    string Email,
    string Phone
) : IRequest<Customer>;
