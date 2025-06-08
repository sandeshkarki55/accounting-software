using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.Infrastructure;

namespace AccountingApi.Features.Accounts;

// Command to delete an account
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

        // Business rule: Cannot delete account if it has sub-accounts
        if (account.SubAccounts.Any())
        {
            throw new InvalidOperationException("Cannot delete account that has sub-accounts. Please delete or reassign sub-accounts first.");
        }

        // Business rule: Cannot delete account if it has journal entry lines
        if (account.JournalEntryLines.Any())
        {
            throw new InvalidOperationException("Cannot delete account that has journal entry lines. Account can only be deactivated.");
        }

        // Business rule: Cannot delete account if it has a non-zero balance
        if (account.Balance != 0)
        {
            throw new InvalidOperationException("Cannot delete account with non-zero balance. Please adjust the balance to zero first.");
        }

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}