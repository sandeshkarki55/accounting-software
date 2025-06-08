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
}