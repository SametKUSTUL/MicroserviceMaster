using CustomerService.Application.Queries;
using CustomerService.Domain.Entities;
using CustomerService.Domain.Repositories;
using MediatR;

namespace CustomerService.Application.Handlers;

public class GetCustomerByCustomerIdHandler : IRequestHandler<GetCustomerByCustomerIdQuery, Customer?>
{
    private readonly ICustomerRepository _repository;

    public GetCustomerByCustomerIdHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<Customer?> Handle(GetCustomerByCustomerIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
    }
}
