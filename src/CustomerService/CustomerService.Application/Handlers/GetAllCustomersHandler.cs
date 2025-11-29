using CustomerService.Application.Queries;
using CustomerService.Domain.Entities;
using CustomerService.Domain.Repositories;
using MediatR;

namespace CustomerService.Application.Handlers;

public class GetAllCustomersHandler : IRequestHandler<GetAllCustomersQuery, List<Customer>>
{
    private readonly ICustomerRepository _repository;

    public GetAllCustomersHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Customer>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }
}
