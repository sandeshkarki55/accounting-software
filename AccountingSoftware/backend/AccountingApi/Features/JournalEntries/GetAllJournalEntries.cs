using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;
using AccountingApi.Infrastructure.Extensions; // Add this using directive

namespace AccountingApi.Features.JournalEntries;

// Query to get all journal entries
public record GetAllJournalEntriesQuery(PaginationParams Pagination, SortingParams Sorting, FilteringParams Filtering) : IRequest<PagedResult<JournalEntryDto>>;

// Handler for GetAllJournalEntriesQuery
public class GetAllJournalEntriesQueryHandler(AccountingDbContext context, JournalEntryMapper journalEntryMapper)
    : IRequestHandler<GetAllJournalEntriesQuery, PagedResult<JournalEntryDto>>
{
    public async Task<PagedResult<JournalEntryDto>> Handle(GetAllJournalEntriesQuery request, CancellationToken cancellationToken)
    {
        var query = context.JournalEntries
            .Include(je => je.Lines)
                .ThenInclude(l => l.Account)
            .Where(je => !je.IsDeleted);

        // Apply Filtering
        if (!string.IsNullOrEmpty(request.Filtering.SearchTerm))
        {
            var searchTerm = request.Filtering.SearchTerm.ToLower();
            query = query.Where(je =>
                je.EntryNumber.ToLower().Contains(searchTerm) ||
                je.Description.ToLower().Contains(searchTerm) ||
                je.Reference.ToLower().Contains(searchTerm)
            );
        }

        if (!string.IsNullOrEmpty(request.Filtering.StatusFilter))
        {
            query = request.Filtering.StatusFilter.ToLower() switch
            {
                "posted" => query.Where(je => je.IsPosted),
                "unposted" => query.Where(je => !je.IsPosted),
                _ => query // "all" or any other value
            };
        }

        // Apply Sorting using extension method
        // Provide a default sort if OrderBy is not specified
        var orderedQuery = string.IsNullOrEmpty(request.Sorting.OrderBy)
            ? query.OrderByDescending(je => je.TransactionDate).ThenByDescending(je => je.EntryNumber)
            : query.ApplySorting(request.Sorting);

        // Get total count before pagination
        var totalCount = await orderedQuery.CountAsync(cancellationToken);

        // Apply Pagination using extension method
        var pagedQuery = orderedQuery.ApplyPagination(request.Pagination);

        var pagedJournalEntries = await pagedQuery.ToListAsync(cancellationToken);

        var items = journalEntryMapper.ToDto(pagedJournalEntries).ToList();

        return new PagedResult<JournalEntryDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.Pagination.PageNumber,
            PageSize = request.Pagination.PageSize
        };
    }
}
