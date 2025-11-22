using Moq;
using NUnit.Framework;
using PaymentService.Application.Commands;
using PaymentService.Application.Configuration;
using PaymentService.Application.Handlers;
using PaymentService.Application.Services;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Repositories;

namespace PaymentService.Tests.Handlers;

[TestFixture]
public class ProcessPaymentHandlerTests
{
    private Mock<IPaymentRepository> _repositoryMock;
    private Mock<IMessagePublisher> _publisherMock;
    private ProcessPaymentHandler _handler;

    [SetUp]
    public void Setup()
    {
        _repositoryMock = new Mock<IPaymentRepository>();
        _publisherMock = new Mock<IMessagePublisher>();
        var settings = new MessagingSettings { PaymentCompletedRoutingKey = "payment.completed" };
        _handler = new ProcessPaymentHandler(_repositoryMock.Object, _publisherMock.Object, settings);
    }

    [Test]
    public async Task Handle_ShouldProcessPayment_WhenValidCommand()
    {
        var command = new ProcessPaymentCommand(Guid.NewGuid(), "customer-123", 100.00m);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment p, CancellationToken ct) => p);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.OrderId, Is.EqualTo(command.OrderId));
        Assert.That(result.CustomerId, Is.EqualTo("customer-123"));
        Assert.That(result.Amount, Is.EqualTo(100.00m));
        Assert.That(result.Status, Is.EqualTo(PaymentStatus.Completed));
        Assert.That(result.ProcessedAt, Is.Not.Null);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Once);
        _publisherMock.Verify(p => p.PublishAsync("payment.completed", It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
