using MediatR;
using ProductService.Domain.Entities;

namespace ProductService.Application.Queries;

public record GetAllProductsQuery : IRequest<List<Product>>;
