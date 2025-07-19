using AccountingApi.Infrastructure;
using AccountingApi.Infrastructure.Extensions;
using AccountingApi.Mappings;

using Microsoft.EntityFrameworkCore;

namespace AccountingApi.Features.Accounts;

// Query to get all accounts with paging, sorting, filtering
using AccountingApi.DTOs;
using MyMediator;
public record GetAllAccountsQuery(
    PaginationParams Pagination,
    SortingParams Sorting,
    AccountFilteringParams Filtering
) : IRequest<PagedResult<AccountDto>>;

// Handler for GetAllAccountsQuery
public class GetAllAccountsQueryHandler(AccountingDbContext context, AccountMapper accountMapper) : IRequestHandler<GetAllAccountsQuery, PagedResult<AccountDto>>
{
    public async Task<PagedResult<AccountDto>> Handle(GetAllAccountsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Accounts.Include(a => a.ParentAccount).AsQueryable();

        // Filtering
        if (!string.IsNullOrWhiteSpace(request.Filtering.SearchTerm))
        {
            var term = request.Filtering.SearchTerm.ToLower();
            query = query.Where(a => a.AccountName.ToLower().Contains(term) || a.AccountCode.ToLower().Contains(term));
        }
        if (request.Filtering.AccountType.HasValue)
        {
            query = query.Where(a => (int)a.AccountType == request.Filtering.AccountType.Value);
        }
        if (request.Filtering.IsActive.HasValue)
        {
            query = query.Where(a => a.IsActive == request.Filtering.IsActive.Value);
        }

        // Sorting
        query = query.ApplySorting(request.Sorting);

        // Total count before paging
        var totalCount = await query.CountAsync(cancellationToken);

        // Paging
        query = query.ApplyPagination(request.Pagination);

        var accounts = await query.ToListAsync(cancellationToken);
        var dtos = accountMapper.ToDto(accounts).ToList();

        return new PagedResult<AccountDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = request.Pagination.PageNumber,
            PageSize = request.Pagination.PageSize
        };
    }
}