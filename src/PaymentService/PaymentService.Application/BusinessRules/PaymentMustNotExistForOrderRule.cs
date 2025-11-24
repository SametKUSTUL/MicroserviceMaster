using PaymentService.Application.Constants;
using PaymentService.Application.Resources;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Repositories;

namespace PaymentService.Application.BusinessRules;

public class PaymentMustNotExistForOrderRule : IBusinessRule
{
    private readonly Guid _orderId;
    private readonly IPaymentRepository _repository;

    public string ErrorCode => ErrorCodes.PaymentAlreadyExists;
    public string ErrorMessage => ErrorMessages.GetString(ErrorCodes.PaymentAlreadyExists);

    public PaymentMustNotExistForOrderRule(Guid orderId, IPaymentRepository repository)
    {
        _orderId = orderId;
        _repository = repository;
    }

    public async Task<bool> IsSatisfiedAsync(CancellationToken cancellationToken = default)
    {
        var existingPayment = await _repository.GetByOrderIdAsync(_orderId, cancellationToken);
        return existingPayment == null || existingPayment.Status != PaymentStatus.Completed;
    }
}
