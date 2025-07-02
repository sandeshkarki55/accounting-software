using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.Infrastructure;

namespace AccountingApi.Features.JournalEntries;

// Command to delete a journal entry (soft delete)
public record DeleteJournalEntryCommand(int Id) : IRequest<bool>;

// Handler for DeleteJournalEntryCommand
public class DeleteJournalEntryCommandHandler : IRequestHandler<DeleteJournalEntryCommand, bool>
{
    private readonly AccountingDbContext _context;

    public DeleteJournalEntryCommandHandler(AccountingDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var journalEntry = await _context.JournalEntries
            .Include(je => je.Lines)
            .FirstOrDefaultAsync(je => je.Id == request.Id, cancellationToken);

        if (journalEntry == null)
            return false;

        // Business rule: Cannot delete posted journal entries
        if (journalEntry.IsPosted)
        {
            throw new InvalidOperationException("Cannot delete a posted journal entry. Posted entries are immutable for audit purposes.");
        }

        // Perform soft delete on journal entry and its lines
        journalEntry.IsDeleted = true;
        journalEntry.DeletedAt = DateTime.UtcNow;
        journalEntry.DeletedBy = "System"; // TODO: Replace with actual user when authentication is implemented
        journalEntry.UpdatedAt = DateTime.UtcNow;
        journalEntry.UpdatedBy = "System";

        // Also soft delete all journal entry lines
        foreach (var line in journalEntry.Lines)
        {
            line.IsDeleted = true;
            line.DeletedAt = DateTime.UtcNow;
            line.DeletedBy = "System";
            line.UpdatedAt = DateTime.UtcNow;
            line.UpdatedBy = "System";
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
