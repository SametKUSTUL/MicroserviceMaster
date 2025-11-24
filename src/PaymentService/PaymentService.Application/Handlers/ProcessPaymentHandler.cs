using MediatR;
using Microsoft.Extensions.Logging;
using PaymentService.Application.BusinessRules;
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
    private readonly ILogger<ProcessPaymentHandler> _logger;

    public ProcessPaymentHandler(IPaymentRepository repository, IMessagePublisher messagePublisher, MessagingSettings settings, ILogger<ProcessPaymentHandler> logger)
    {
        _repository = repository;
        _messagePublisher = messagePublisher;
        _settings = settings;
        _logger = logger;
    }

    public async Task<Payment> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing payment for OrderId: {OrderId}, Amount: {Amount}", request.OrderId, request.Amount);
        
        await ValidateBusinessRulesAsync(request, cancellationToken);

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
        _logger.LogInformation("Payment created with Id: {PaymentId}", result.Id);

        await Task.Delay(1000, cancellationToken);

        result.Status = PaymentStatus.Completed;
        result.ProcessedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(result, cancellationToken);
        _logger.LogInformation("Payment completed for OrderId: {OrderId}", result.OrderId);

        await _messagePublisher.PublishAsync(_settings.PaymentCompletedRoutingKey, new
        {
            PaymentId = result.Id,
            OrderId = result.OrderId,
            Amount = result.Amount,
            Status = result.Status.ToString()
        }, cancellationToken);

        return result;
    }

    private async Task ValidateBusinessRulesAsync(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var rules = new List<IBusinessRule>
        {
            new CustomerIdMustBeValidRule(request.CustomerId),
            new PaymentAmountMustBeValidRule(request.Amount),
            new PaymentMustNotExistForOrderRule(request.OrderId, _repository)
        };

        await BusinessRuleValidator.ValidateAsync(rules, cancellationToken);
    }
}
