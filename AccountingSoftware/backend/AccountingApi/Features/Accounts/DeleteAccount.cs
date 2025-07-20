using AccountingApi.Infrastructure;
using AccountingApi.Services.CurrentUserService;

using Microsoft.EntityFrameworkCore;

namespace AccountingApi.Features.Accounts;

// Command to delete an account (soft delete)
using MyMediator;

public record DeleteAccountCommand(int Id) : IRequest<bool>;

// Handler for DeleteAccountCommand
public class DeleteAccountCommandHandler(AccountingDbContext context, ICurrentUserService currentUserService) : IRequestHandler<DeleteAccountCommand, bool>
{
    public async Task<bool> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await context.Accounts
            .Include(a => a.SubAccounts)
            .Include(a => a.JournalEntryLines) // Updated to use correct navigation property
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (account == null)
            return false;

        // Business rule: Cannot delete account if it has non-deleted sub-accounts
        if (account.SubAccounts.Any(sa => !sa.IsDeleted))
        {
            throw new InvalidOperationException("Cannot delete account that has active sub-accounts. Please delete or reassign sub-accounts first.");
        }

        // Business rule: Cannot delete account if it has journal entry lines
        if (account.JournalEntryLines.Any(jel => !jel.IsDeleted))
        {
            throw new InvalidOperationException("Cannot delete account that has active journal entry lines. Account can only be deactivated.");
        }

        // Business rule: Cannot delete account if it has a non-zero balance
        if (account.Balance != 0)
        {
            throw new InvalidOperationException("Cannot delete account with non-zero balance. Please adjust the balance to zero first.");
        }

        // Get current user for audit
        var currentUser = currentUserService.GetCurrentUserForAudit();

        // Perform soft delete
        account.IsDeleted = true;
        account.DeletedAt = DateTime.UtcNow;
        account.DeletedBy = currentUser;
        account.UpdatedAt = DateTime.UtcNow;
        account.UpdatedBy = currentUser;

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}