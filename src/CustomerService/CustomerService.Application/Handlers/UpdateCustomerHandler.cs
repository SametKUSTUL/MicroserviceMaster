using CustomerService.Application.Commands;
using CustomerService.Domain.Entities;
using CustomerService.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerService.Application.Handlers;

public class UpdateCustomerHandler : IRequestHandler<UpdateCustomerCommand, Customer>
{
    private readonly ICustomerRepository _repository;
    private readonly ILogger<UpdateCustomerHandler> _logger;

    public UpdateCustomerHandler(ICustomerRepository repository, ILogger<UpdateCustomerHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Customer> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (customer == null)
            throw new Exception($"Customer not found: {request.Id}");

        customer.Name = request.Name;
        customer.Surname = request.Surname;
        customer.Email = request.Email;
        customer.Phone = request.Phone;
        customer.Status = request.Status;
        customer.UpdatedAt = DateTime.UtcNow;

        var result = await _repository.UpdateAsync(customer, cancellationToken);
        _logger.LogInformation("Customer updated: {Id}", result.Id);

        return result;
    }
}
