using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.Infrastructure;

namespace AccountingApi.Features.JournalEntries;

// Command to delete a journal entry line (soft delete)
public record DeleteJournalEntryLineCommand(int Id) : IRequest<bool>;

// Handler for DeleteJournalEntryLineCommand
public class DeleteJournalEntryLineCommandHandler : IRequestHandler<DeleteJournalEntryLineCommand, bool>
{
    private readonly AccountingDbContext _context;

    public DeleteJournalEntryLineCommandHandler(AccountingDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteJournalEntryLineCommand request, CancellationToken cancellationToken)
    {
        var journalEntryLine = await _context.JournalEntryLines
            .Include(jel => jel.JournalEntry)
            .FirstOrDefaultAsync(jel => jel.Id == request.Id, cancellationToken);

        if (journalEntryLine == null)
            return false;

        // Business rule: Cannot delete lines from posted journal entries
        if (journalEntryLine.JournalEntry.IsPosted)
        {
            throw new InvalidOperationException("Cannot delete lines from a posted journal entry. Posted entries are immutable for audit purposes.");
        }

        // Business rule: Must maintain balanced journal entry
        var remainingLines = await _context.JournalEntryLines
            .Where(jel => jel.JournalEntryId == journalEntryLine.JournalEntryId && 
                         jel.Id != request.Id && 
                         !jel.IsDeleted)
            .ToListAsync(cancellationToken);

        if (remainingLines.Count < 1)
        {
            throw new InvalidOperationException("Cannot delete the last remaining line from a journal entry. Delete the entire journal entry instead.");
        }

        // Check if remaining lines would still be balanced after this deletion
        var remainingDebits = remainingLines.Sum(l => l.DebitAmount);
        var remainingCredits = remainingLines.Sum(l => l.CreditAmount);
        
        if (remainingDebits != remainingCredits)
        {
            throw new InvalidOperationException("Cannot delete this journal entry line as it would leave the journal entry unbalanced. The sum of debits must equal the sum of credits.");
        }

        // Perform soft delete
        journalEntryLine.IsDeleted = true;
        journalEntryLine.DeletedAt = DateTime.UtcNow;
        journalEntryLine.DeletedBy = "System"; // TODO: Replace with actual user when authentication is implemented
        journalEntryLine.UpdatedAt = DateTime.UtcNow;
        journalEntryLine.UpdatedBy = "System";

        // Update the journal entry total amount
        var journalEntry = journalEntryLine.JournalEntry;
        journalEntry.TotalAmount = remainingLines.Sum(l => Math.Max(l.DebitAmount, l.CreditAmount));
        journalEntry.UpdatedAt = DateTime.UtcNow;
        journalEntry.UpdatedBy = "System";

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
