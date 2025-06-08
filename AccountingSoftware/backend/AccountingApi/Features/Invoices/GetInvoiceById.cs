using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Models;
using AccountingApi.Mappings;

namespace AccountingApi.Features.Invoices;

// Query to get an invoice by ID
public record GetInvoiceByIdQuery(int Id) : IRequest<InvoiceDto?>;

// Handler for GetInvoiceByIdQuery
public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceDto?>
{
    private readonly AccountingDbContext _context;
    private readonly InvoiceMapper _invoiceMapper;

    public GetInvoiceByIdQueryHandler(AccountingDbContext context, InvoiceMapper invoiceMapper)
    {
        _context = context;
        _invoiceMapper = invoiceMapper;
    }

    public async Task<InvoiceDto?> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.CompanyInfo)
            .Include(i => i.Items.OrderBy(item => item.SortOrder))
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (invoice == null)
            return null;

        return _invoiceMapper.ToDto(invoice);
    }
}