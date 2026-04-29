using AccountingApi.Constants;
using AccountingApi.DTOs;
using AccountingApi.Features.Invoices;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MyMediator;

namespace AccountingApi.Controllers;

public class InvoicesController(IMediator mediator) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<InvoiceDto>>> GetInvoices([
        FromQuery] PaginationParams pagination,
        [FromQuery] SortingParams sorting,
        [FromQuery] InvoiceFilteringParams filtering)
    {
        var result = await mediator.Send(new GetAllInvoicesQuery(pagination, sorting, filtering));
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InvoiceDto>> GetInvoice(int id)
    {
        var invoice = await mediator.Send(new GetInvoiceByIdQuery(id));

        if (invoice == null)
            return NotFound();

        return Ok(invoice);
    }

    /// <summary>
    /// Create a new invoice. Requires Admin, Manager, or Accountant role.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.Accountant}")]
    public async Task<ActionResult<InvoiceDto>> CreateInvoice(CreateInvoiceDto createInvoiceDto)
    {
        var invoice = await mediator.Send(new CreateInvoiceCommand(createInvoiceDto));
        return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, invoice);
    }

    /// <summary>
    /// Mark an invoice as paid. Requires Admin, Manager, or Accountant role.
    /// </summary>
    [HttpPost("{id}/mark-as-paid")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.Accountant}")]
    public async Task<ActionResult<InvoiceDto>> MarkInvoiceAsPaid(int id, MarkInvoiceAsPaidDto markAsPaidDto)
    {
        var invoice = await mediator.Send(new MarkInvoiceAsPaidCommand(id, markAsPaidDto.PaidDate, markAsPaidDto.PaymentReference));
        return Ok(invoice);
    }

    /// <summary>
    /// Delete an invoice. Requires Admin, Manager, or Accountant role.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.Accountant}")]
    public async Task<IActionResult> DeleteInvoice(int id)
    {
        var success = await mediator.Send(new DeleteInvoiceCommand(id));

        if (!success)
        {
            return NotFound(new { message = $"Invoice with ID {id} not found." });
        }

        return NoContent();
    }

    /// <summary>
    /// Delete an invoice line item. Requires Admin, Manager, or Accountant role.
    /// </summary>
    [HttpDelete("items/{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.Accountant}")]
    public async Task<IActionResult> DeleteInvoiceItem(int id)
    {
        var result = await mediator.Send(new DeleteInvoiceItemCommand(id));
        if (!result)
            return NotFound(new { message = "Invoice item not found." });

        return NoContent();
    }
}