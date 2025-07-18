using AccountingApi.DTOs;
using AccountingApi.Features.Customers;

using Microsoft.AspNetCore.Mvc;

using MyMediator;

namespace AccountingApi.Controllers;

public class CustomersController(IMediator mediator) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<CustomerDto>>> GetCustomers([
        FromQuery] PaginationParams pagination,
        [FromQuery] SortingParams sorting,
        [FromQuery] CustomerFilteringParams filtering)
    {
        var result = await mediator.Send(new GetAllCustomersQuery(pagination, sorting, filtering));
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
    {
        var customer = await mediator.Send(new GetCustomerByIdQuery(id));

        if (customer == null)
            return NotFound();

        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> CreateCustomer(CreateCustomerDto createCustomerDto)
    {
        var customer = await mediator.Send(new CreateCustomerCommand(createCustomerDto));
        return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(int id, UpdateCustomerDto updateCustomerDto)
    {
        var customer = await mediator.Send(new UpdateCustomerCommand(id, updateCustomerDto));

        if (customer == null)
            return NotFound();

        return Ok(customer);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var success = await mediator.Send(new DeleteCustomerCommand(id));

        if (!success)
        {
            return NotFound(new { message = $"Customer with ID {id} not found." });
        }

        return NoContent();
    }
}