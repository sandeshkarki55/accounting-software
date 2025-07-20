using AccountingApi.Infrastructure;
using AccountingApi.Models;

using Microsoft.EntityFrameworkCore;

using MyMediator;

namespace AccountingApi.Features.Invoices;

// Query to get all invoices with paging, sorting, and filtering
using AccountingApi.DTOs;
using AccountingApi.Infrastructure.Extensions;

public record GetAllInvoicesQuery(PaginationParams Pagination, SortingParams Sorting, InvoiceFilteringParams Filtering) : IRequest<PagedResult<InvoiceDto>>;

// Handler for GetAllInvoicesQuery
public class GetAllInvoicesQueryHandler(AccountingDbContext context) : IRequestHandler<GetAllInvoicesQuery, PagedResult<InvoiceDto>>
{
    public async Task<PagedResult<InvoiceDto>> Handle(GetAllInvoicesQuery request, CancellationToken cancellationToken)
    {
        var query = context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.CompanyInfo)
            .Include(i => i.Items.OrderBy(item => item.SortOrder))
            .Where(i => !i.IsDeleted);

        // Apply Filtering
        if (!string.IsNullOrEmpty(request.Filtering.SearchTerm))
        {
            var searchTerm = request.Filtering.SearchTerm.ToLower();
            query = query.Where(i =>
                i.InvoiceNumber.ToLower().Contains(searchTerm) ||
                i.Description.ToLower().Contains(searchTerm) ||
                i.Customer.CompanyName.ToLower().Contains(searchTerm)
            );
        }

        if (!string.IsNullOrEmpty(request.Filtering.StatusFilter))
        {
            query = request.Filtering.StatusFilter.ToLower() switch
            {
                "paid" => query.Where(i => i.Status == InvoiceStatus.Paid),
                "unpaid" => query.Where(i => i.Status != InvoiceStatus.Paid),
                "overdue" => query.Where(i => i.Status == InvoiceStatus.Overdue),
                "draft" => query.Where(i => i.Status == InvoiceStatus.Draft),
                "cancelled" => query.Where(i => i.Status == InvoiceStatus.Cancelled),
                _ => query // "all" or any other value
            };
        }

        // Apply Sorting using extension method
        var orderedQuery = string.IsNullOrEmpty(request.Sorting.OrderBy)
            ? query.OrderByDescending(i => i.InvoiceDate).ThenByDescending(i => i.InvoiceNumber)
            : query.ApplySorting(request.Sorting);

        // Get total count before pagination
        var totalCount = await orderedQuery.CountAsync(cancellationToken);

        // Apply Pagination
        var pagedQuery = orderedQuery
            .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize);

        var pagedInvoices = await pagedQuery.ToListAsync(cancellationToken);

        var items = pagedInvoices.Select(i => new InvoiceDto
        {
            Id = i.Id,
            InvoiceNumber = i.InvoiceNumber,
            InvoiceDate = i.InvoiceDate,
            DueDate = i.DueDate,
            CustomerId = i.CustomerId,
            CustomerName = i.Customer.CompanyName,
            CompanyInfoId = i.CompanyInfoId,
            CompanyName = i.CompanyInfo != null ? i.CompanyInfo.CompanyName : null,
            Description = i.Description,
            SubTotal = i.SubTotal,
            TaxRate = i.TaxRate,
            TaxAmount = i.TaxAmount,
            DiscountAmount = i.DiscountAmount,
            TotalAmount = i.TotalAmount,
            Status = i.Status,
            StatusName = i.Status.ToString(),
            Notes = i.Notes,
            Terms = i.Terms,
            PaidDate = i.PaidDate,
            PaymentReference = i.PaymentReference,
            Items = i.Items.Select(item => new InvoiceItemDto
            {
                Id = item.Id,
                InvoiceId = item.InvoiceId,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Amount = item.Amount,
                SortOrder = item.SortOrder
            }).ToList()
        }).ToList();

        return new PagedResult<InvoiceDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.Pagination.PageNumber,
            PageSize = request.Pagination.PageSize
        };
    }

    // No custom sorting helper needed; use ApplySorting extension
}