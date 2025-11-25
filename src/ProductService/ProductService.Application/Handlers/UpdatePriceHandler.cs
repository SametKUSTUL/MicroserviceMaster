using MediatR;
using ProductService.Application.Commands;
using ProductService.Application.Exceptions;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Handlers;

public class UpdatePriceHandler : IRequestHandler<UpdatePriceCommand, Unit>
{
    private readonly IProductRepository _repository;

    public UpdatePriceHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(UpdatePriceCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
            throw new NotFoundException($"Product {request.ProductId} not found");

        product.CurrentPrice = request.NewPrice;
        product.UpdatedAt = DateTime.UtcNow;
        product.PriceHistories.Add(new PriceHistory
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Price = request.NewPrice,
            EffectiveDate = DateTime.UtcNow
        });

        await _repository.UpdateAsync(product, cancellationToken);
        return Unit.Value;
    }
}
