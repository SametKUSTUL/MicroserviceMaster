using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OrderService.Application.Commands;
using OrderService.Application.Factories;
using OrderService.Application.Handlers;
using OrderService.Application.Services;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;

namespace OrderService.Tests.Handlers;

[TestFixture]
public class CreateOrderHandlerTests
{
    private Mock<IOrderRepository> _repositoryMock;
    private Mock<IOrderFactory> _orderFactoryMock;
    private Mock<IOrderEventPublisher> _eventPublisherMock;
    private Mock<ILogger<CreateOrderHandler>> _loggerMock;
    private Mock<IProductService> _productServiceMock;
    private CreateOrderHandler _handler;

    [SetUp]
    public void Setup()
    {
        _repositoryMock = new Mock<IOrderRepository>();
        _orderFactoryMock = new Mock<IOrderFactory>();
        _eventPublisherMock = new Mock<IOrderEventPublisher>();
        _loggerMock = new Mock<ILogger<CreateOrderHandler>>();
        _productServiceMock = new Mock<IProductService>();
        _productServiceMock.Setup(p => p.ValidateProductAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _handler = new CreateOrderHandler(_repositoryMock.Object, _orderFactoryMock.Object, _eventPublisherMock.Object, _loggerMock.Object, _productServiceMock.Object);
    }

    [Test]
    public async Task Handle_ShouldCreateOrder_WhenValidCommand()
    {
        var command = new CreateOrderCommand(
            "customer-123",
            new List<OrderItemDto>
            {
                new("product-1", 2, 10.50m),
                new("product-2", 1, 25.00m)
            }
        );

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "customer-123",
            Status = OrderStatus.Pending,
            TotalAmount = 46.00m,
            Items = new List<OrderItem>
            {
                new() { ProductId = "product-1", Quantity = 2, Price = 10.50m },
                new() { ProductId = "product-2", Quantity = 1, Price = 25.00m }
            }
        };

        _orderFactoryMock.Setup(f => f.CreateOrder(command)).Returns(order);
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>())).ReturnsAsync((Order o, CancellationToken ct) => o);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.CustomerId, Is.EqualTo("customer-123"));
        Assert.That(result.TotalAmount, Is.EqualTo(46.00m));
        Assert.That(result.Status, Is.EqualTo(OrderStatus.Pending));
        Assert.That(result.Items.Count, Is.EqualTo(2));

        _orderFactoryMock.Verify(f => f.CreateOrder(command), Times.Once);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _eventPublisherMock.Verify(p => p.PublishOrderEventsAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldCalculateTotalAmount_Correctly()
    {
        var command = new CreateOrderCommand(
            "customer-456",
            new List<OrderItemDto>
            {
                new("product-1", 3, 15.00m)
            }
        );

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "customer-456",
            Status = OrderStatus.Pending,
            TotalAmount = 45.00m,
            Items = new List<OrderItem> { new() { ProductId = "product-1", Quantity = 3, Price = 15.00m } }
        };

        _orderFactoryMock.Setup(f => f.CreateOrder(command)).Returns(order);
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>())).ReturnsAsync((Order o, CancellationToken ct) => o);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.TotalAmount, Is.EqualTo(45.00m));
    }
}
