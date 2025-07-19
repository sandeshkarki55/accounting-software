using AccountingApi.Constants;
using AccountingApi.DTOs;
using AccountingApi.Features.Accounts;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MyMediator;

namespace AccountingApi.Controllers;

public class AccountsController(IMediator mediator) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<AccountDto>>> GetAccounts([
        FromQuery] PaginationParams pagination,
        [FromQuery] SortingParams sorting,
        [FromQuery] AccountFilteringParams filtering)
    {
        var result = await mediator.Send(new GetAllAccountsQuery(pagination, sorting, filtering));
        return Ok(result);
    }

    [HttpGet("hierarchy")]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccountsHierarchy()
    {
        var accounts = await mediator.Send(new GetAccountsHierarchyQuery());
        return Ok(accounts);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AccountDto>> GetAccount(int id)
    {
        var account = await mediator.Send(new GetAccountByIdQuery(id));

        if (account == null)
        {
            return NotFound(new { message = $"Account with ID {id} not found." });
        }

        return Ok(account);
    }

    /// <summary>
    /// Create a new account. Only users with Admin, Manager, or Accountant roles can create accounts.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AccountDto>> CreateAccount(CreateAccountDto createAccountDto)
    {
        var account = await mediator.Send(new CreateAccountCommand(createAccountDto));
        return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, account);
    }

    /// <summary>
    /// Update an existing account. Only users with Admin or Manager roles can update accounts.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<IActionResult> UpdateAccount(int id, UpdateAccountDto updateAccountDto)
    {
        var success = await mediator.Send(new UpdateAccountCommand(id, updateAccountDto));

        if (!success)
        {
            return NotFound(new { message = $"Account with ID {id} not found." });
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        var success = await mediator.Send(new DeleteAccountCommand(id));

        if (!success)
        {
            return NotFound(new { message = $"Account with ID {id} not found." });
        }

        return NoContent();
    }
}