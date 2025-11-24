using FluentValidation;
using PaymentService.Application.Commands;

namespace PaymentService.Application.Validators;

public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required")
            .NotNull().WithMessage("Customer ID cannot be null");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0");
    }
}
