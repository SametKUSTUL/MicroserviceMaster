using MediatR;

namespace ProductService.Application.Commands;

public record UpdatePriceCommand(Guid ProductId, decimal NewPrice) : IRequest<Unit>;
