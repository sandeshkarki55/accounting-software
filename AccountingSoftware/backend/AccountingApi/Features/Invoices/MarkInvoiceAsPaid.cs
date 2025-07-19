using MyMediator;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Models;
using AccountingApi.Services;

namespace AccountingApi.Features.Invoices;

// Command to mark an invoice as paid
public record MarkInvoiceAsPaidCommand(int InvoiceId, DateTime PaidDate, string? PaymentReference = null) : IRequest<InvoiceDto>;

// Handler for MarkInvoiceAsPaidCommand
public class MarkInvoiceAsPaidCommandHandler(
    AccountingDbContext context,
    IAutomaticJournalEntryService automaticJournalEntryService,
    ICurrentUserService currentUserService) : IRequestHandler<MarkInvoiceAsPaidCommand, InvoiceDto>
{
    public async Task<InvoiceDto> Handle(MarkInvoiceAsPaidCommand request, CancellationToken cancellationToken)
    {
        var invoice = await context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.CompanyInfo)
            .Include(i => i.Items.OrderBy(item => item.SortOrder))
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

        if (invoice == null)
            throw new InvalidOperationException($"Invoice with ID {request.InvoiceId} not found.");

        if (invoice.Status == InvoiceStatus.Paid)
            throw new InvalidOperationException("Invoice is already marked as paid.");

        if (invoice.Status == InvoiceStatus.Cancelled)
            throw new InvalidOperationException("Cannot mark a cancelled invoice as paid.");

        // Update invoice payment information
        invoice.Status = InvoiceStatus.Paid;
        invoice.PaidDate = request.PaidDate;
        invoice.PaymentReference = request.PaymentReference;
        invoice.UpdatedAt = DateTime.UtcNow;
        invoice.UpdatedBy = currentUserService.GetCurrentUserForAudit();

        await context.SaveChangesAsync(cancellationToken);

        // Create automatic journal entry for the payment
        try
        {
            await automaticJournalEntryService.CreatePaymentJournalEntryAsync(invoice, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the payment
            // In a production system, you might want to use a proper logging framework
            Console.WriteLine($"Warning: Failed to create automatic journal entry for payment of invoice {invoice.InvoiceNumber}: {ex.Message}");
        }

        // Return updated invoice DTO
        return new InvoiceDto
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            CustomerId = invoice.CustomerId,
            CustomerName = invoice.Customer.CompanyName,
            CompanyInfoId = invoice.CompanyInfoId,
            CompanyName = invoice.CompanyInfo?.CompanyName,
            Description = invoice.Description,
            SubTotal = invoice.SubTotal,
            TaxRate = invoice.TaxRate,
            TaxAmount = invoice.TaxAmount,
            DiscountAmount = invoice.DiscountAmount,
            TotalAmount = invoice.TotalAmount,
            Status = invoice.Status,
            StatusName = invoice.Status.ToString(),
            Notes = invoice.Notes,
            Terms = invoice.Terms,
            PaidDate = invoice.PaidDate,
            PaymentReference = invoice.PaymentReference,
            Items = invoice.Items.Select(item => new InvoiceItemDto
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