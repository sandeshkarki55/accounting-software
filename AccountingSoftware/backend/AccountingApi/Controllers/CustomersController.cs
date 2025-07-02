using Microsoft.AspNetCore.Mvc;
using MediatR;
using AccountingApi.DTOs;
using AccountingApi.Features.Customers;

namespace AccountingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
    {
        var customers = await _mediator.Send(new GetAllCustomersQuery());
        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
    {
        var customer = await _mediator.Send(new GetCustomerByIdQuery(id));
        
        if (customer == null)
            return NotFound();

        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> CreateCustomer(CreateCustomerDto createCustomerDto)
    {
        var customer = await _mediator.Send(new CreateCustomerCommand(createCustomerDto));
        return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(int id, UpdateCustomerDto updateCustomerDto)
    {
        var customer = await _mediator.Send(new UpdateCustomerCommand(id, updateCustomerDto));
        
        if (customer == null)
            return NotFound();

        return Ok(customer);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        try
        {
            var success = await _mediator.Send(new DeleteCustomerCommand(id));
            
            if (!success)
            {
                return NotFound(new { message = $"Customer with ID {id} not found." });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the customer.", details = ex.Message });
        }
    }
}