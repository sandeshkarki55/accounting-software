using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Models;

namespace AccountingApi.Features.Invoices;

// Query to get all invoices
public record GetAllInvoicesQuery : IRequest<List<InvoiceDto>>;

// Handler for GetAllInvoicesQuery
public class GetAllInvoicesQueryHandler(AccountingDbContext context) : IRequestHandler<GetAllInvoicesQuery, List<InvoiceDto>>
{
    public async Task<List<InvoiceDto>> Handle(GetAllInvoicesQuery request, CancellationToken cancellationToken)
    {
        var invoices = await context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.CompanyInfo)
            .Include(i => i.Items.OrderBy(item => item.SortOrder))
            .OrderByDescending(i => i.InvoiceDate)
            .Select(i => new InvoiceDto
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
            })
            .ToListAsync(cancellationToken);

        return invoices;
    }
}