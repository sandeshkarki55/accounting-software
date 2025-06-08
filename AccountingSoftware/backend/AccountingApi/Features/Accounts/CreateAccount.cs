using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Models;
using AccountingApi.Mappings;

namespace AccountingApi.Features.Accounts;

// Command to create a new account
public record CreateAccountCommand(CreateAccountDto Account) : IRequest<AccountDto>;

// Handler for CreateAccountCommand
public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, AccountDto>
{
    private readonly AccountingDbContext _context;
    private readonly AccountMapper _accountMapper;

    public CreateAccountCommandHandler(AccountingDbContext context, AccountMapper accountMapper)
    {
        _context = context;
        _accountMapper = accountMapper;
    }

    public async Task<AccountDto> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        // Validate account code uniqueness
        var existingAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountCode == request.Account.AccountCode, cancellationToken);

        if (existingAccount != null)
        {
            throw new InvalidOperationException($"Account with code '{request.Account.AccountCode}' already exists.");
        }

        // Validate parent account exists if specified
        if (request.Account.ParentAccountId.HasValue)
        {
            var parentExists = await _context.Accounts
                .AnyAsync(a => a.Id == request.Account.ParentAccountId.Value, cancellationToken);

            if (!parentExists)
            {
                throw new InvalidOperationException($"Parent account with ID '{request.Account.ParentAccountId}' does not exist.");
            }
        }

        // Create new account entity using mapper
        var account = _accountMapper.ToEntity(request.Account);

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);

        // Return the created account as DTO using mapper
        return _accountMapper.ToDto(account);
    }
}