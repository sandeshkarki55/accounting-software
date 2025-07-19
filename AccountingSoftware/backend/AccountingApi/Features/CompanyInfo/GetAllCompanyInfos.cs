using MyMediator;
using Microsoft.EntityFrameworkCore;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;

namespace AccountingApi.Features.CompanyInfo;

using AccountingApi.DTOs;
using AccountingApi.Infrastructure.Extensions;

// Query to get all company info records with paging, sorting, filtering
public record GetAllCompanyInfosQuery(PaginationParams Pagination, SortingParams Sorting, CompanyInfoFilteringParams Filtering) : IRequest<PagedResult<CompanyInfoDto>>;

// Handler for GetAllCompanyInfosQuery
public class GetAllCompanyInfosQueryHandler(AccountingDbContext context, CompanyInfoMapper mapper) : IRequestHandler<GetAllCompanyInfosQuery, PagedResult<CompanyInfoDto>>
{
    public async Task<PagedResult<CompanyInfoDto>> Handle(GetAllCompanyInfosQuery request, CancellationToken cancellationToken)
    {
        var query = context.CompanyInfos.AsQueryable();

        // Filtering
        if (!string.IsNullOrEmpty(request.Filtering.SearchTerm))
        {
            var searchTerm = request.Filtering.SearchTerm.ToLower();
            query = query.Where(c =>
                c.CompanyName.ToLower().Contains(searchTerm) ||
                (c.LegalName != null && c.LegalName.ToLower().Contains(searchTerm)) ||
                (c.Email != null && c.Email.ToLower().Contains(searchTerm)) ||
                (c.Phone != null && c.Phone.ToLower().Contains(searchTerm))
            );
        }

        // Sorting
        var orderedQuery = string.IsNullOrEmpty(request.Sorting.OrderBy)
            ? query.OrderByDescending(c => c.IsDefault).ThenBy(c => c.CompanyName)
            : query.ApplySorting(request.Sorting);

        // Total count before paging
        var totalCount = await orderedQuery.CountAsync(cancellationToken);

        // Paging
        var pagedQuery = orderedQuery
            .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize);

        var pagedCompanies = await pagedQuery.ToListAsync(cancellationToken);
        var items = mapper.ToDto(pagedCompanies).ToList();

        return new PagedResult<CompanyInfoDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.Pagination.PageNumber,
            PageSize = request.Pagination.PageSize
        };
    }
}