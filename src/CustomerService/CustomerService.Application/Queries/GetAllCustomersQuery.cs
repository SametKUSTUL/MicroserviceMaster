using CustomerService.Domain.Entities;
using MediatR;

namespace CustomerService.Application.Queries;

public record GetAllCustomersQuery : IRequest<List<Customer>>;
