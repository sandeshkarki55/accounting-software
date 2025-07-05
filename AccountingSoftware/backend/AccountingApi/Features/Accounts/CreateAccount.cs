using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Models;
using AccountingApi.Mappings;
using AccountingApi.Services;

namespace AccountingApi.Features.Accounts;

// Command to create a new account
public record CreateAccountCommand(CreateAccountDto Account) : IRequest<AccountDto>;

// Handler for CreateAccountCommand
public class CreateAccountCommandHandler(AccountingDbContext context, AccountMapper accountMapper, ICurrentUserService currentUserService, ILogger<CreateAccountCommandHandler> logger) : IRequestHandler<CreateAccountCommand, AccountDto>
{
    public async Task<AccountDto> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        // Validate account code uniqueness
        var existingAccount = await context.Accounts
            .FirstOrDefaultAsync(a => a.AccountCode == request.Account.AccountCode, cancellationToken);

        if (existingAccount != null)
        {
            throw new InvalidOperationException($"Account with code '{request.Account.AccountCode}' already exists.");
        }

        // Validate parent account exists if specified
        if (request.Account.ParentAccountId.HasValue)
        {
            var parentExists = await context.Accounts
                .AnyAsync(a => a.Id == request.Account.ParentAccountId.Value, cancellationToken);

            if (!parentExists)
            {
                throw new InvalidOperationException($"Parent account with ID '{request.Account.ParentAccountId}' does not exist.");
            }
        }

        // Create new account entity using mapper
        var account = accountMapper.ToEntity(request.Account);
        
        // Set audit information
        var currentUser = currentUserService.GetCurrentUserForAudit();
        account.CreatedBy = currentUser;
        account.UpdatedBy = currentUser;

        context.Accounts.Add(account);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Account created successfully by {User}. Account Id: {accountId}", currentUser, account.Id);

        // Return the created account as DTO using mapper
        return accountMapper.ToDto(account);
    }
}