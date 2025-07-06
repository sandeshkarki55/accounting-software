using Microsoft.AspNetCore.Mvc;
using MediatR;
using AccountingApi.DTOs;
using AccountingApi.Features.Invoices;

namespace AccountingApi.Controllers;

public class InvoicesController(IMediator mediator) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetInvoices()
    {
        var invoices = await mediator.Send(new GetAllInvoicesQuery());
        return Ok(invoices);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InvoiceDto>> GetInvoice(int id)
    {
        var invoice = await mediator.Send(new GetInvoiceByIdQuery(id));
        
        if (invoice == null)
            return NotFound();

        return Ok(invoice);
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceDto>> CreateInvoice(CreateInvoiceDto createInvoiceDto)
    {
        var invoice = await mediator.Send(new CreateInvoiceCommand(createInvoiceDto));
        return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, invoice);
    }

    [HttpPost("{id}/mark-as-paid")]
    public async Task<ActionResult<InvoiceDto>> MarkInvoiceAsPaid(int id, MarkInvoiceAsPaidDto markAsPaidDto)
    {
        var invoice = await mediator.Send(new MarkInvoiceAsPaidCommand(id, markAsPaidDto.PaidDate, markAsPaidDto.PaymentReference));
        return Ok(invoice);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInvoice(int id)
    {
        var success = await mediator.Send(new DeleteInvoiceCommand(id));

        if (!success)
        {
            return NotFound(new { message = $"Invoice with ID {id} not found." });
        }

        return NoContent();
    }

    [HttpDelete("items/{id:int}")]
    public async Task<IActionResult> DeleteInvoiceItem(int id)
    {
        var result = await mediator.Send(new DeleteInvoiceItemCommand(id));
        if (!result)
            return NotFound(new { message = "Invoice item not found." });

        return NoContent();
    }
}