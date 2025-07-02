using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.Infrastructure;

namespace AccountingApi.Features.Accounts;

// Command to delete an account (soft delete)
public record DeleteAccountCommand(int Id) : IRequest<bool>;

// Handler for DeleteAccountCommand
public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, bool>
{
    private readonly AccountingDbContext _context;

    public DeleteAccountCommandHandler(AccountingDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.Accounts
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

        // Perform soft delete
        account.IsDeleted = true;
        account.DeletedAt = DateTime.UtcNow;
        account.DeletedBy = "System"; // TODO: Replace with actual user when authentication is implemented
        account.UpdatedAt = DateTime.UtcNow;
        account.UpdatedBy = "System";

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}