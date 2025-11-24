using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.BusinessRules;
using OrderService.Application.Commands;
using OrderService.Application.Configuration;
using OrderService.Application.Services;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Handlers;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Order>
{
    private readonly IOrderRepository _repository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly MessagingSettings _settings;
    private readonly ILogger<CreateOrderHandler> _logger;

    public CreateOrderHandler(IOrderRepository repository, IMessagePublisher messagePublisher, MessagingSettings settings, ILogger<CreateOrderHandler> logger)
    {
        _repository = repository;
        _messagePublisher = messagePublisher;
        _settings = settings;
        _logger = logger;
    }

    public async Task<Order> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating order for CustomerId: {CustomerId}", request.CustomerId);
        _logger.LogInformation($"CreateOrderCommand : {JsonSerializer.Serialize(request)}");
        
        await ValidateBusinessRulesAsync(request, cancellationToken);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Items = request.Items.Select(i => new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList()
        };

        order.TotalAmount = order.Items.Sum(i => i.Price * i.Quantity);

        var result = await _repository.AddAsync(order, cancellationToken);
        _logger.LogInformation("Order created with Id: {OrderId}, TotalAmount: {TotalAmount}", result.Id, result.TotalAmount);
        
        _logger.LogInformation($"Message Publish with : {_settings.OrderCreatedRoutingKey} routing key.");
        await _messagePublisher.PublishAsync(_settings.OrderCreatedRoutingKey, new
        {
            OrderId = result.Id,
            CustomerId = result.CustomerId,
            TotalAmount = result.TotalAmount
        }, cancellationToken);

        return result;
    }

    private async Task ValidateBusinessRulesAsync(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var rules = new List<IBusinessRule>
        {
            new CustomerIdMustBeValidRule(request.CustomerId),
            new OrderMustHaveItemsRule(request.Items),
            new OrderTotalAmountMustBeValidRule(request.Items)
        };

        await BusinessRuleValidator.ValidateAsync(rules, cancellationToken);
    }
}
