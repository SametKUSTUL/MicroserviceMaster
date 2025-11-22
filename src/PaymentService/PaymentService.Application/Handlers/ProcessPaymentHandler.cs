using MediatR;
using PaymentService.Application.Commands;
using PaymentService.Application.Configuration;
using PaymentService.Application.Services;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Repositories;

namespace PaymentService.Application.Handlers;

public class ProcessPaymentHandler : IRequestHandler<ProcessPaymentCommand, Payment>
{
    private readonly IPaymentRepository _repository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly MessagingSettings _settings;

    public ProcessPaymentHandler(IPaymentRepository repository, IMessagePublisher messagePublisher, MessagingSettings settings)
    {
        _repository = repository;
        _messagePublisher = messagePublisher;
        _settings = settings;
    }

    public async Task<Payment> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = request.OrderId,
            CustomerId = request.CustomerId,
            Amount = request.Amount,
            Status = PaymentStatus.Processing,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _repository.AddAsync(payment, cancellationToken);

        await Task.Delay(1000, cancellationToken);

        result.Status = PaymentStatus.Completed;
        result.ProcessedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(result, cancellationToken);

        await _messagePublisher.PublishAsync(_settings.PaymentCompletedRoutingKey, new
        {
            PaymentId = result.Id,
            OrderId = result.OrderId,
            Amount = result.Amount,
            Status = result.Status.ToString()
        }, cancellationToken);

        return result;
    }
}
