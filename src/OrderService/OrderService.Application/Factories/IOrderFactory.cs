using OrderService.Application.Commands;
using OrderService.Domain.Entities;

namespace OrderService.Application.Factories;

public interface IOrderFactory
{
    Order CreateOrder(CreateOrderCommand command);
}
