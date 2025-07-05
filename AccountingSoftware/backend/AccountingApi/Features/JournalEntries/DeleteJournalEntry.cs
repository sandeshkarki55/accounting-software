using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.Infrastructure;
using AccountingApi.Services;

namespace AccountingApi.Features.JournalEntries;

// Command to delete a journal entry (soft delete)
public record DeleteJournalEntryCommand(int Id) : IRequest<bool>;

// Handler for DeleteJournalEntryCommand
public class DeleteJournalEntryCommandHandler(AccountingDbContext context, ICurrentUserService currentUserService) : IRequestHandler<DeleteJournalEntryCommand, bool>
{
    public async Task<bool> Handle(DeleteJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var journalEntry = await context.JournalEntries
            .Include(je => je.Lines)
            .FirstOrDefaultAsync(je => je.Id == request.Id, cancellationToken);

        if (journalEntry == null)
            return false;

        // Business rule: Cannot delete posted journal entries
        if (journalEntry.IsPosted)
        {
            throw new InvalidOperationException("Cannot delete a posted journal entry. Posted entries are immutable for audit purposes.");
        }

        var currentUser = currentUserService.GetCurrentUserForAudit();

        // Perform soft delete on journal entry and its lines
        journalEntry.IsDeleted = true;
        journalEntry.DeletedAt = DateTime.UtcNow;
        journalEntry.DeletedBy = currentUser;
        journalEntry.UpdatedAt = DateTime.UtcNow;
        journalEntry.UpdatedBy = currentUser;

        // Also soft delete all journal entry lines
        foreach (var line in journalEntry.Lines)
        {
            line.IsDeleted = true;
            line.DeletedAt = DateTime.UtcNow;
            line.DeletedBy = currentUser;
            line.UpdatedAt = DateTime.UtcNow;
            line.UpdatedBy = currentUser;
        }

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
