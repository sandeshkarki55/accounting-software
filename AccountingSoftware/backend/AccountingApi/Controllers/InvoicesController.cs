using Microsoft.AspNetCore.Mvc;
using MediatR;
using AccountingApi.DTOs;
using AccountingApi.Features.Invoices;

namespace AccountingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvoicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetInvoices()
    {
        var invoices = await _mediator.Send(new GetAllInvoicesQuery());
        return Ok(invoices);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InvoiceDto>> GetInvoice(int id)
    {
        var invoice = await _mediator.Send(new GetInvoiceByIdQuery(id));
        
        if (invoice == null)
            return NotFound();

        return Ok(invoice);
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceDto>> CreateInvoice(CreateInvoiceDto createInvoiceDto)
    {
        var invoice = await _mediator.Send(new CreateInvoiceCommand(createInvoiceDto));
        return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, invoice);
    }

    [HttpPost("{id}/mark-as-paid")]
    public async Task<ActionResult<InvoiceDto>> MarkInvoiceAsPaid(int id, MarkInvoiceAsPaidDto markAsPaidDto)
    {
        try
        {
            var invoice = await _mediator.Send(new MarkInvoiceAsPaidCommand(id, markAsPaidDto.PaidDate, markAsPaidDto.PaymentReference));
            return Ok(invoice);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}