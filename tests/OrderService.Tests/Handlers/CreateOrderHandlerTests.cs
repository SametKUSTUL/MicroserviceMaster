using Moq;
using NUnit.Framework;
using OrderService.Application.Commands;
using OrderService.Application.Handlers;
using OrderService.Application.Services;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;

namespace OrderService.Tests.Handlers;

[TestFixture]
public class CreateOrderHandlerTests
{
    private Mock<IOrderRepository> _repositoryMock;
    private Mock<IMessagePublisher> _publisherMock;
    private CreateOrderHandler _handler;

    [SetUp]
    public void Setup()
    {
        _repositoryMock = new Mock<IOrderRepository>();
        _publisherMock = new Mock<IMessagePublisher>();
        _handler = new CreateOrderHandler(_repositoryMock.Object, _publisherMock.Object);
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

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order o, CancellationToken ct) => o);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.CustomerId, Is.EqualTo("customer-123"));
        Assert.That(result.TotalAmount, Is.EqualTo(46.00m));
        Assert.That(result.Status, Is.EqualTo(OrderStatus.Pending));
        Assert.That(result.Items.Count, Is.EqualTo(2));

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _publisherMock.Verify(p => p.PublishAsync("order.created", It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
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

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order o, CancellationToken ct) => o);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.TotalAmount, Is.EqualTo(45.00m));
    }
}
