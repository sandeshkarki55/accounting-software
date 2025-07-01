using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Models;
using AccountingApi.Services;

namespace AccountingApi.Features.Invoices;

// Command to create an invoice
public record CreateInvoiceCommand(CreateInvoiceDto Invoice) : IRequest<InvoiceDto>;

// Handler for CreateInvoiceCommand
public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, InvoiceDto>
{
    private readonly AccountingDbContext _context;
    private readonly INumberGenerationService _numberGenerationService;

    public CreateInvoiceCommandHandler(AccountingDbContext context, INumberGenerationService numberGenerationService)
    {
        _context = context;
        _numberGenerationService = numberGenerationService;
    }

    public async Task<InvoiceDto> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        // Generate invoice number
        var invoiceNumber = await _numberGenerationService.GenerateInvoiceNumberAsync();

        // Calculate totals from items
        var subTotal = request.Invoice.Items.Sum(item => item.Quantity * item.UnitPrice);
        var taxAmount = subTotal * request.Invoice.TaxRate;
        var totalAmount = subTotal + taxAmount - request.Invoice.DiscountAmount;

        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            InvoiceDate = request.Invoice.InvoiceDate,
            DueDate = request.Invoice.DueDate,
            CustomerId = request.Invoice.CustomerId,
            CompanyInfoId = request.Invoice.CompanyInfoId,
            Description = request.Invoice.Description,
            SubTotal = subTotal,
            TaxRate = request.Invoice.TaxRate,
            TaxAmount = taxAmount,
            DiscountAmount = request.Invoice.DiscountAmount,
            TotalAmount = totalAmount,
            Notes = request.Invoice.Notes,
            Terms = request.Invoice.Terms,
            CreatedBy = "System", // TODO: Replace with actual user when authentication is implemented
            UpdatedBy = "System"
        };

        // Add invoice items
        foreach (var itemDto in request.Invoice.Items)
        {
            var item = new InvoiceItem
            {
                Description = itemDto.Description,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice,
                Amount = itemDto.Quantity * itemDto.UnitPrice,
                SortOrder = itemDto.SortOrder,
                CreatedBy = "System",
                UpdatedBy = "System"
            };
            invoice.Items.Add(item);
        }

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync(cancellationToken);

        // Load the created invoice with related data
        var createdInvoice = await _context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.CompanyInfo)
            .Include(i => i.Items.OrderBy(item => item.SortOrder))
            .FirstAsync(i => i.Id == invoice.Id, cancellationToken);

        return new InvoiceDto
        {
            Id = createdInvoice.Id,
            InvoiceNumber = createdInvoice.InvoiceNumber,
            InvoiceDate = createdInvoice.InvoiceDate,
            DueDate = createdInvoice.DueDate,
            CustomerId = createdInvoice.CustomerId,
            CustomerName = createdInvoice.Customer.CompanyName,
            CompanyInfoId = createdInvoice.CompanyInfoId,
            CompanyName = createdInvoice.CompanyInfo?.CompanyName,
            Description = createdInvoice.Description,
            SubTotal = createdInvoice.SubTotal,
            TaxRate = createdInvoice.TaxRate,
            TaxAmount = createdInvoice.TaxAmount,
            DiscountAmount = createdInvoice.DiscountAmount,
            TotalAmount = createdInvoice.TotalAmount,
            Status = createdInvoice.Status,
            StatusName = createdInvoice.Status.ToString(),
            Notes = createdInvoice.Notes,
            Terms = createdInvoice.Terms,
            PaidDate = createdInvoice.PaidDate,
            PaymentReference = createdInvoice.PaymentReference,
            Items = createdInvoice.Items.Select(item => new InvoiceItemDto
            {
                Id = item.Id,
                InvoiceId = item.InvoiceId,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Amount = item.Amount,
                SortOrder = item.SortOrder
            }).ToList()
        };
    }
}