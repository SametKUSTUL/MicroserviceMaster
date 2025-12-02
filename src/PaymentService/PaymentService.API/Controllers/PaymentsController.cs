using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.Commands;
using PaymentService.Application.Queries;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var payments = await _mediator.Send(new GetAllPaymentsQuery(), cancellationToken);
        return Ok(payments);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var payment = await _mediator.Send(new GetPaymentByIdQuery(id), cancellationToken);
        return payment == null ? NotFound() : Ok(payment);
    }

    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetByOrderId(Guid orderId, CancellationToken cancellationToken)
    {
        var payment = await _mediator.Send(new GetPaymentByOrderIdQuery(orderId), cancellationToken);
        return payment == null ? NotFound() : Ok(payment);
    }

    [HttpPost]
    public async Task<IActionResult> Process([FromBody] ProcessPaymentCommand command, CancellationToken cancellationToken)
    {
        var payment = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
    }
}
