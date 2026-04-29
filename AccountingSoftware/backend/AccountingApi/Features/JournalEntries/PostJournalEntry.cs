using AccountingApi.Infrastructure;
using AccountingApi.Models;
using AccountingApi.Services.CurrentUserService;

using Microsoft.EntityFrameworkCore;

using MyMediator;

namespace AccountingApi.Features.JournalEntries;

// Command to post a journal entry
public record PostJournalEntryCommand(int JournalEntryId) : IRequest<bool>;

// Handler for PostJournalEntryCommand
public class PostJournalEntryCommandHandler(AccountingDbContext context, ICurrentUserService currentUserService) : IRequestHandler<PostJournalEntryCommand, bool>
{
    public async Task<bool> Handle(PostJournalEntryCommand request, CancellationToken cancellationToken)
    {
        // Get the journal entry with its lines
        var journalEntry = await context.JournalEntries
            .Include(je => je.Lines)
            .FirstOrDefaultAsync(je => je.Id == request.JournalEntryId && !je.IsDeleted, cancellationToken);

        if (journalEntry == null)
        {
            throw new InvalidOperationException("Journal entry not found or has been deleted.");
        }

        if (journalEntry.IsPosted)
        {
            throw new InvalidOperationException("Journal entry is already posted.");
        }

        // Validate that the entry is balanced and each line has a single entry
        JournalEntryValidationHelper.ValidateBalanced(journalEntry.Lines);
        JournalEntryValidationHelper.ValidateSingleEntryPerLine(journalEntry.Lines);

        var currentUser = currentUserService.GetCurrentUserForAudit();

        // Post the journal entry
        journalEntry.IsPosted = true;
        journalEntry.PostedAt = DateTime.UtcNow;
        journalEntry.PostedBy = currentUser;
        journalEntry.UpdatedAt = DateTime.UtcNow;
        journalEntry.UpdatedBy = currentUser;

        // Update account balances
        await UpdateAccountBalancesAsync(journalEntry, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task UpdateAccountBalancesAsync(JournalEntry journalEntry, CancellationToken cancellationToken)
    {
        var currentUser = currentUserService.GetCurrentUserForAudit();
        var accountIds = journalEntry.Lines.Select(l => l.AccountId).Distinct();
        var accounts = await context.Accounts
            .Where(a => accountIds.Contains(a.Id))
            .ToListAsync(cancellationToken);

        foreach (var account in accounts)
        {
            var lines = journalEntry.Lines.Where(l => l.AccountId == account.Id);
            var debitTotal = lines.Sum(l => l.DebitAmount);
            var creditTotal = lines.Sum(l => l.CreditAmount);

            // Update balance based on account type
            switch (account.AccountType)
            {
                case AccountType.Asset:
                case AccountType.Expense:
                    // Assets and Expenses increase with debits
                    account.Balance += (debitTotal - creditTotal);
                    break;

                case AccountType.Liability:
                case AccountType.Equity:
                case AccountType.Revenue:
                    // Liabilities, Equity, and Revenue increase with credits
                    account.Balance += (creditTotal - debitTotal);
                    break;
            }

            account.UpdatedAt = DateTime.UtcNow;
            account.UpdatedBy = currentUser;
        }
    }
}