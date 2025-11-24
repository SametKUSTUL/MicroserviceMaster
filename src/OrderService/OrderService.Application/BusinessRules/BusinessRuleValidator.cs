using OrderService.Application.Exceptions;

namespace OrderService.Application.BusinessRules;

public static class BusinessRuleValidator
{
    public static async Task ValidateAsync(IBusinessRule rule, CancellationToken cancellationToken = default)
    {
        if (!await rule.IsSatisfiedAsync(cancellationToken))
        {
            throw new BusinessRuleException(rule.ErrorCode, rule.ErrorMessage);
        }
    }

    public static async Task ValidateAsync(IEnumerable<IBusinessRule> rules, CancellationToken cancellationToken = default)
    {
        foreach (var rule in rules)
        {
            await ValidateAsync(rule, cancellationToken);
        }
    }
}
