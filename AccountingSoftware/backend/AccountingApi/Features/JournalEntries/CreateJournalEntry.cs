using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Models;
using AccountingApi.Mappings;
using AccountingApi.Services;

namespace AccountingApi.Features.JournalEntries;

// Command to create a journal entry
public record CreateJournalEntryCommand(CreateJournalEntryDto JournalEntry) : IRequest<JournalEntryDto>;

// Handler for CreateJournalEntryCommand
public class CreateJournalEntryCommandHandler(
    AccountingDbContext context, 
    JournalEntryMapper journalEntryMapper,
    ICurrentUserService currentUserService) : IRequestHandler<CreateJournalEntryCommand, JournalEntryDto>
{
    public async Task<JournalEntryDto> Handle(CreateJournalEntryCommand request, CancellationToken cancellationToken)
    {
        // Validate that debits equal credits
        var totalDebits = request.JournalEntry.Lines.Sum(l => l.DebitAmount);
        var totalCredits = request.JournalEntry.Lines.Sum(l => l.CreditAmount);
        
        if (Math.Abs(totalDebits - totalCredits) > 0.01m) // Allow for small rounding differences
        {
            throw new InvalidOperationException($"Journal entry is not balanced. Debits: {totalDebits:C}, Credits: {totalCredits:C}");
        }

        // Validate that all lines have either debit or credit (not both, not neither)
        foreach (var line in request.JournalEntry.Lines)
        {
            if ((line.DebitAmount > 0 && line.CreditAmount > 0) || 
                (line.DebitAmount == 0 && line.CreditAmount == 0))
            {
                throw new InvalidOperationException("Each journal entry line must have either a debit amount or credit amount (but not both or neither).");
            }
        }

        // Validate that all referenced accounts exist
        var accountIds = request.JournalEntry.Lines.Select(l => l.AccountId).Distinct().ToList();
        var existingAccountIds = await context.Accounts
            .Where(a => accountIds.Contains(a.Id) && !a.IsDeleted)
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);

        var missingAccountIds = accountIds.Except(existingAccountIds).ToList();
        if (missingAccountIds.Any())
        {
            throw new InvalidOperationException($"Invalid account IDs: {string.Join(", ", missingAccountIds)}");
        }

        // Generate journal entry number
        var entryNumber = await GenerateJournalEntryNumberAsync(cancellationToken);
        var currentUser = currentUserService.GetCurrentUserForAudit();

        // Create the journal entry
        var journalEntry = journalEntryMapper.ToEntity(request.JournalEntry);
        journalEntry.EntryNumber = entryNumber;
        journalEntry.CreatedBy = currentUser;
        journalEntry.UpdatedBy = currentUser;

        // Set created/updated info for lines
        foreach (var line in journalEntry.Lines)
        {
            line.CreatedBy = currentUser;
            line.UpdatedBy = currentUser;
        }

        context.JournalEntries.Add(journalEntry);
        await context.SaveChangesAsync(cancellationToken);

        // Update account balances if the entry is posted
        if (journalEntry.IsPosted)
        {
            await UpdateAccountBalancesAsync(journalEntry, cancellationToken);
        }

        // Load the created journal entry with related data
        var createdJournalEntry = await context.JournalEntries
            .Include(je => je.Lines)
                .ThenInclude(l => l.Account)
            .FirstAsync(je => je.Id == journalEntry.Id, cancellationToken);

        return journalEntryMapper.ToDto(createdJournalEntry);
    }

    private async Task<string> GenerateJournalEntryNumberAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow;
        var prefix = $"JE{today:yyyyMM}";
        
        // Use IgnoreQueryFilters to include soft-deleted entries when determining the next number
        // This ensures we don't reuse numbers even if entries are soft-deleted
        var lastEntry = await context.JournalEntries
            .IgnoreQueryFilters() // This bypasses the global IsDeleted filter
            .Where(je => je.EntryNumber.StartsWith(prefix))
            .OrderByDescending(je => je.EntryNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastEntry == null)
        {
            return $"{prefix}0001";
        }

        // Extract the sequence number
        var lastNumber = lastEntry.EntryNumber.Substring(prefix.Length);
        if (int.TryParse(lastNumber, out var sequence))
        {
            return $"{prefix}{(sequence + 1):D4}";
        }

        return $"{prefix}0001";
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

        await context.SaveChangesAsync(cancellationToken);
    }
}
