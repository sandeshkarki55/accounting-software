using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Models;

namespace AccountingApi.Features.Accounts;

// Command to create a new account
public record CreateAccountCommand(CreateAccountDto Account) : IRequest<AccountDto>;

// Handler for CreateAccountCommand
public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, AccountDto>
{
    private readonly AccountingDbContext _context;

    public CreateAccountCommandHandler(AccountingDbContext context)
    {
        _context = context;
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

        // Create new account entity
        var account = new Account
        {
            AccountCode = request.Account.AccountCode,
            AccountName = request.Account.AccountName,
            AccountType = request.Account.AccountType,
            Description = request.Account.Description,
            ParentAccountId = request.Account.ParentAccountId,
            Balance = 0, // New accounts start with zero balance
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);

        // Return the created account as DTO
        return new AccountDto
        {
            Id = account.Id,
            AccountCode = account.AccountCode,
            AccountName = account.AccountName,
            AccountType = account.AccountType,
            Balance = account.Balance,
            IsActive = account.IsActive,
            Description = account.Description,
            ParentAccountId = account.ParentAccountId
        };
    }
}