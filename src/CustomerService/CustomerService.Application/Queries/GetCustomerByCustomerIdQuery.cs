using CustomerService.Domain.Entities;
using MediatR;

namespace CustomerService.Application.Queries;

public record GetCustomerByCustomerIdQuery(string CustomerId) : IRequest<Customer?>;
