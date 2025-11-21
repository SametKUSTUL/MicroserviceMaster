using MediatR;
using OrderService.Domain.Entities;

namespace OrderService.Application.Commands;

public record CreateOrderCommand(string CustomerId, List<OrderItemDto> Items) : IRequest<Order>;

public record OrderItemDto(string ProductId, int Quantity, decimal Price);
