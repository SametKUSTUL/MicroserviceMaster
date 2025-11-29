using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.BusinessRules;
using OrderService.Application.Commands;
using OrderService.Application.Factories;
using OrderService.Application.Services;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Handlers;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Order>
{
    private readonly IOrderRepository _repository;
    private readonly IOrderFactory _orderFactory;
    private readonly IOrderEventPublisher _eventPublisher;
    private readonly ILogger<CreateOrderHandler> _logger;
    private readonly IProductService _productService;
    private readonly ICustomerService _customerService;

    public CreateOrderHandler(IOrderRepository repository, IOrderFactory orderFactory, IOrderEventPublisher eventPublisher, ILogger<CreateOrderHandler> logger, IProductService productService, ICustomerService customerService)
    {
        _repository = repository;
        _orderFactory = orderFactory;
        _eventPublisher = eventPublisher;
        _logger = logger;
        _productService = productService;
        _customerService = customerService;
    }

    public async Task<Order> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating order for CustomerId: {CustomerId}", request.CustomerId);
        
        await ValidateBusinessRulesAsync(request, cancellationToken);

        var order = _orderFactory.CreateOrder(request);
        var result = await _repository.AddAsync(order, cancellationToken);
        
        _logger.LogInformation("Order created with Id: {OrderId}, TotalAmount: {TotalAmount}", result.Id, result.TotalAmount);
        
        await _eventPublisher.PublishOrderEventsAsync(result, cancellationToken);

        return result;
    }

    private async Task ValidateBusinessRulesAsync(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var rules = new List<IBusinessRule>
        {
            new CustomerIdMustBeValidRule(request.CustomerId),
            new CustomerMustExistRule(_customerService, request.CustomerId),
            new OrderMustHaveItemsRule(request.Items),
            new OrderTotalAmountMustBeValidRule(request.Items)
        };

        foreach (var item in request.Items)
        {
            rules.Add(new ProductMustExistRule(_productService, item.ProductId));
            rules.Add(new ProductStockMustBeSufficientRule(_productService, item.ProductId, item.Quantity));
        }

        await BusinessRuleValidator.ValidateAsync(rules, cancellationToken);
    }
}
