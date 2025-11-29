using CustomerService.Application.Commands;
using CustomerService.Domain.Entities;
using CustomerService.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerService.Application.Handlers;

public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, Customer>
{
    private readonly ICustomerRepository _repository;
    private readonly ILogger<CreateCustomerHandler> _logger;

    public CreateCustomerHandler(ICustomerRepository repository, ILogger<CreateCustomerHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Customer> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating customer: {CustomerId}", request.CustomerId);

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Name = request.Name,
            Surname = request.Surname,
            Email = request.Email,
            Phone = request.Phone,
            Status = CustomerStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _repository.AddAsync(customer, cancellationToken);
        _logger.LogInformation("Customer created with Id: {Id}", result.Id);

        return result;
    }
}
