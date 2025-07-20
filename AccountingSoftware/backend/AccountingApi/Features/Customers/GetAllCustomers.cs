using AccountingApi.Infrastructure;
using AccountingApi.Infrastructure.Extensions;
using AccountingApi.Mappings;

using Microsoft.EntityFrameworkCore;

using MyMediator;

namespace AccountingApi.Features.Customers;

// Query to get all customers with paging, sorting, filtering
using AccountingApi.DTOs;

public record GetAllCustomersQuery(
    PaginationParams Pagination,
    SortingParams Sorting,
    CustomerFilteringParams Filtering
) : IRequest<PagedResult<CustomerDto>>;

// Handler for GetAllCustomersQuery
public class GetAllCustomersQueryHandler(
    AccountingDbContext context,
    CustomerMapper customerMapper)
    : IRequestHandler<GetAllCustomersQuery, PagedResult<CustomerDto>>
{
    public async Task<PagedResult<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var query = context.Customers.AsQueryable();

        // Filtering
        if (!string.IsNullOrWhiteSpace(request.Filtering.SearchTerm))
        {
            var term = request.Filtering.SearchTerm.ToLower();
            query = query.Where(c => c.CompanyName.ToLower().Contains(term) || (c.ContactPersonName != null && c.ContactPersonName.ToLower().Contains(term)));
        }
        if (request.Filtering.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == request.Filtering.IsActive.Value);
        }

        // Sorting
        query = query.ApplySorting(request.Sorting);

        // Total count before paging
        var totalCount = await query.CountAsync(cancellationToken);

        // Paging
        query = query.ApplyPagination(request.Pagination);

        var customers = await query.ToListAsync(cancellationToken);
        var dtos = customerMapper.ToDto(customers).ToList();

        return new PagedResult<CustomerDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = request.Pagination.PageNumber,
            PageSize = request.Pagination.PageSize
        };
    }
}