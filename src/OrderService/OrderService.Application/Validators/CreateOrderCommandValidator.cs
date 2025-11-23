using FluentValidation;
using OrderService.Application.Commands;

namespace OrderService.Application.Validators;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required")
            .NotNull().WithMessage("Customer ID cannot be null");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must have at least one item")
            .NotNull().WithMessage("Items cannot be null");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");

            item.RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0");
        });
    }
}
