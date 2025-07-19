using MyMediator;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Models;
using AccountingApi.Mappings;
using AccountingApi.Services;

namespace AccountingApi.Features.JournalEntries;

// Command to update a journal entry
public record UpdateJournalEntryCommand(int Id, UpdateJournalEntryDto JournalEntry) : IRequest<JournalEntryDto>;

// Handler for UpdateJournalEntryCommand
public class UpdateJournalEntryCommandHandler(
    AccountingDbContext context, 
    JournalEntryMapper journalEntryMapper,
    ICurrentUserService currentUserService) : IRequestHandler<UpdateJournalEntryCommand, JournalEntryDto>
{
    public async Task<JournalEntryDto> Handle(UpdateJournalEntryCommand request, CancellationToken cancellationToken)
    {
        // Find the existing journal entry
        var existingJournalEntry = await context.JournalEntries
            .Include(je => je.Lines)
            .FirstOrDefaultAsync(je => je.Id == request.Id && !je.IsDeleted, cancellationToken);

        if (existingJournalEntry == null)
        {
            throw new InvalidOperationException("Journal entry not found.");
        }

        // Check if the journal entry is posted - posted entries cannot be modified
        if (existingJournalEntry.IsPosted)
        {
            throw new InvalidOperationException("Cannot update a posted journal entry.");
        }

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

        var currentUser = currentUserService.GetCurrentUserForAudit();

        // Update the journal entry properties
        existingJournalEntry.TransactionDate = request.JournalEntry.TransactionDate;
        existingJournalEntry.Description = request.JournalEntry.Description;
        existingJournalEntry.Reference = request.JournalEntry.Reference;
        existingJournalEntry.UpdatedAt = DateTime.UtcNow;
        existingJournalEntry.UpdatedBy = currentUser;

        // Remove all existing lines
        context.JournalEntryLines.RemoveRange(existingJournalEntry.Lines);

        // Add new lines
        foreach (var lineDto in request.JournalEntry.Lines)
        {
            var line = new JournalEntryLine
            {
                JournalEntryId = existingJournalEntry.Id,
                AccountId = lineDto.AccountId,
                DebitAmount = lineDto.DebitAmount,
                CreditAmount = lineDto.CreditAmount,
                Description = lineDto.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = currentUser,
                UpdatedBy = currentUser
            };
            existingJournalEntry.Lines.Add(line);
        }

        // Calculate total amount
        existingJournalEntry.TotalAmount = request.JournalEntry.Lines.Sum(l => Math.Max(l.DebitAmount, l.CreditAmount));

        await context.SaveChangesAsync(cancellationToken);

        // Load the updated journal entry with related data
        var updatedJournalEntry = await context.JournalEntries
            .Include(je => je.Lines)
                .ThenInclude(l => l.Account)
            .FirstAsync(je => je.Id == existingJournalEntry.Id, cancellationToken);

        return journalEntryMapper.ToDto(updatedJournalEntry);
    }
}
