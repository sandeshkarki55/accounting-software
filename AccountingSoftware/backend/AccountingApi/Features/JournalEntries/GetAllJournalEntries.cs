using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;

namespace AccountingApi.Features.JournalEntries;

// Query to get all journal entries
public record GetAllJournalEntriesQuery : IRequest<List<JournalEntryDto>>;

// Handler for GetAllJournalEntriesQuery
public class GetAllJournalEntriesQueryHandler(AccountingDbContext context, JournalEntryMapper journalEntryMapper) 
    : IRequestHandler<GetAllJournalEntriesQuery, List<JournalEntryDto>>
{
    public async Task<List<JournalEntryDto>> Handle(GetAllJournalEntriesQuery request, CancellationToken cancellationToken)
    {
        var journalEntries = await context.JournalEntries
            .Include(je => je.Lines)
                .ThenInclude(l => l.Account)
            .Where(je => !je.IsDeleted)
            .OrderByDescending(je => je.TransactionDate)
            .ThenByDescending(je => je.EntryNumber)
            .ToListAsync(cancellationToken);

        return journalEntryMapper.ToDto(journalEntries).ToList();
    }
}
