using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;
using AccountingApi.Services;

namespace AccountingApi.Features.Accounts;

// Command to update an existing account
public record UpdateAccountCommand(int Id, UpdateAccountDto Account) : IRequest<bool>;

// Handler for UpdateAccountCommand
public class UpdateAccountCommandHandler(AccountingDbContext context, AccountMapper accountMapper, ICurrentUserService currentUserService) : IRequestHandler<UpdateAccountCommand, bool>
{
    public async Task<bool> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await context.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (account == null)
            return false;

        // Update account using mapper
        accountMapper.UpdateEntity(account, request.Account);
        
        // Update audit information
        account.UpdatedBy = currentUserService.GetCurrentUserForAudit();
        account.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}