using CustomerService.Domain.Entities;
using MediatR;

namespace CustomerService.Application.Queries;

public record GetCustomerByIdQuery(Guid Id) : IRequest<Customer?>;
