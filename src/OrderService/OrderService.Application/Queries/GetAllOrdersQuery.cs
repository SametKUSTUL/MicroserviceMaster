using MediatR;
using OrderService.Domain.Entities;

namespace OrderService.Application.Queries;

public record GetAllOrdersQuery : IRequest<IEnumerable<Order>>;
