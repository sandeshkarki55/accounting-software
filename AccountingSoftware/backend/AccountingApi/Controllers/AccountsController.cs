using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using AccountingApi.DTOs;
using AccountingApi.Features.Accounts;
using AccountingApi.Constants;

namespace AccountingApi.Controllers;

public class AccountsController(IMediator mediator) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccounts()
    {
        try
        {
            var accounts = await mediator.Send(new GetAllAccountsQuery());
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
            var accounts = await mediator.Send(new GetAccountsHierarchyQuery());
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
            var account = await mediator.Send(new GetAccountByIdQuery(id));
            
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

    /// <summary>
    /// Create a new account. Only users with Admin, Manager, or Accountant roles can create accounts.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AccountDto>> CreateAccount(CreateAccountDto createAccountDto)
    {
        try
        {
            var account = await mediator.Send(new CreateAccountCommand(createAccountDto));
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

    /// <summary>
    /// Update an existing account. Only users with Admin or Manager roles can update accounts.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<IActionResult> UpdateAccount(int id, UpdateAccountDto updateAccountDto)
    {
        try
        {
            var success = await mediator.Send(new UpdateAccountCommand(id, updateAccountDto));
            
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
            var success = await mediator.Send(new DeleteAccountCommand(id));
            
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