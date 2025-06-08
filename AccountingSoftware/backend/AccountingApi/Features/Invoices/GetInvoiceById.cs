using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Models;

namespace AccountingApi.Features.Invoices;

// Query to get an invoice by ID
public record GetInvoiceByIdQuery(int Id) : IRequest<InvoiceDto?>;

// Handler for GetInvoiceByIdQuery
public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceDto?>
{
    private readonly AccountingDbContext _context;

    public GetInvoiceByIdQueryHandler(AccountingDbContext context)
    {
        _context = context;
    }

    public async Task<InvoiceDto?> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.CompanyInfo)
            .Include(i => i.Items.OrderBy(item => item.SortOrder))
            .Where(i => i.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);

        return invoice;
    }
}