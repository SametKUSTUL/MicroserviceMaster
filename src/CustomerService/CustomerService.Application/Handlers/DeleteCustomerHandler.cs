using CustomerService.Application.Commands;
using CustomerService.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustomerService.Application.Handlers;

public class DeleteCustomerHandler : IRequestHandler<DeleteCustomerCommand, bool>
{
    private readonly ICustomerRepository _repository;
    private readonly ILogger<DeleteCustomerHandler> _logger;

    public DeleteCustomerHandler(ICustomerRepository repository, ILogger<DeleteCustomerHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting customer: {Id}", request.Id);

        var customer = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (customer == null)
        {
            _logger.LogWarning("Customer not found: {Id}", request.Id);
            return false;
        }

        await _repository.DeleteAsync(customer, cancellationToken);
        _logger.LogInformation("Customer deleted: {Id}", request.Id);

        return true;
    }
}
