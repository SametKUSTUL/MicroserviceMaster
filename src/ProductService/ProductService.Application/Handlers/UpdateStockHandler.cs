using MediatR;
using ProductService.Application.Commands;
using ProductService.Application.Exceptions;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Handlers;

public class UpdateStockHandler : IRequestHandler<UpdateStockCommand, Unit>
{
    private readonly IProductRepository _repository;

    public UpdateStockHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(UpdateStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
            throw new NotFoundException($"Product {request.ProductId} not found");

        await _repository.UpdateStockAsync(request.ProductId, request.Quantity, cancellationToken);
        return Unit.Value;
    }
}
