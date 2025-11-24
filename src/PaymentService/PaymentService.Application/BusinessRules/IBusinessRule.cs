namespace PaymentService.Application.BusinessRules;

public interface IBusinessRule
{
    string ErrorCode { get; }
    string ErrorMessage { get; }
    Task<bool> IsSatisfiedAsync(CancellationToken cancellationToken = default);
}
