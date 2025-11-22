using MediatR;
using OrderService.Domain.Entities;

namespace OrderService.Application.Queries;

public record GetOrderByIdQuery(Guid Id) : IRequest<Order?>;
