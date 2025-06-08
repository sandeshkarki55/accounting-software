using Microsoft.AspNetCore.Mvc;
using MediatR;
using AccountingApi.DTOs;
using AccountingApi.Features.Accounts;

namespace AccountingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccounts()
    {
        try
        {
            var accounts = await _mediator.Send(new GetAllAccountsQuery());
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving accounts.", details = ex.Message });
        }
    }

    [HttpGet("hierarchy")]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccountsHierarchy()
    {
        try
        {
            var accounts = await _mediator.Send(new GetAccountsHierarchyQuery());
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving accounts hierarchy.", details = ex.Message });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AccountDto>> GetAccount(int id)
    {
        try
        {
            var account = await _mediator.Send(new GetAccountByIdQuery(id));
            
            if (account == null)
            {
                return NotFound(new { message = $"Account with ID {id} not found." });
            }

            return Ok(account);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving the account.", details = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<AccountDto>> CreateAccount(CreateAccountDto createAccountDto)
    {
        try
        {
            var account = await _mediator.Send(new CreateAccountCommand(createAccountDto));
            return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, account);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the account.", details = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAccount(int id, UpdateAccountDto updateAccountDto)
    {
        try
        {
            var success = await _mediator.Send(new UpdateAccountCommand(id, updateAccountDto));
            
            if (!success)
            {
                return NotFound(new { message = $"Account with ID {id} not found." });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the account.", details = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        try
        {
            var success = await _mediator.Send(new DeleteAccountCommand(id));
            
            if (!success)
            {
                return NotFound(new { message = $"Account with ID {id} not found." });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the account.", details = ex.Message });
        }
    }
}