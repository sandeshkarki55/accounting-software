using MyMediator;
using Microsoft.EntityFrameworkCore;
using AccountingApi.Infrastructure;
using AccountingApi.Models;
using AccountingApi.Services.CurrentUserService;

namespace AccountingApi.Features.Invoices;

// Command to delete an invoice item (soft delete)
public record DeleteInvoiceItemCommand(int Id) : IRequest<bool>;

// Handler for DeleteInvoiceItemCommand
public class DeleteInvoiceItemCommandHandler(AccountingDbContext context, ICurrentUserService currentUserService) : IRequestHandler<DeleteInvoiceItemCommand, bool>
{
    public async Task<bool> Handle(DeleteInvoiceItemCommand request, CancellationToken cancellationToken)
    {
        var invoiceItem = await context.InvoiceItems
            .Include(ii => ii.Invoice)
            .FirstOrDefaultAsync(ii => ii.Id == request.Id, cancellationToken);

        if (invoiceItem == null)
            return false;

        // Business rule: Cannot delete items from paid invoices
        if (invoiceItem.Invoice.Status == InvoiceStatus.Paid)
        {
            throw new InvalidOperationException("Cannot delete items from a paid invoice. Paid invoices can only be archived through other means.");
        }

        // Business rule: Must have at least one item on an invoice
        var remainingItems = await context.InvoiceItems
            .Where(ii => ii.InvoiceId == invoiceItem.InvoiceId && 
                        ii.Id != request.Id && 
                        !ii.IsDeleted)
            .CountAsync(cancellationToken);

        if (remainingItems == 0)
        {
            throw new InvalidOperationException("Cannot delete the last remaining item from an invoice. Delete the entire invoice instead.");
        }

        var currentUser = currentUserService.GetCurrentUserForAudit();

        // Perform soft delete
        invoiceItem.IsDeleted = true;
        invoiceItem.DeletedAt = DateTime.UtcNow;
        invoiceItem.DeletedBy = currentUser;
        invoiceItem.UpdatedAt = DateTime.UtcNow;
        invoiceItem.UpdatedBy = currentUser;

        // Update invoice totals
        var invoice = invoiceItem.Invoice;
        var activeItems = await context.InvoiceItems
            .Where(ii => ii.InvoiceId == invoice.Id && !ii.IsDeleted && ii.Id != request.Id)
            .ToListAsync(cancellationToken);

        // Recalculate invoice totals
        invoice.SubTotal = activeItems.Sum(i => i.Amount);
        invoice.TaxAmount = invoice.SubTotal * (invoice.TaxRate / 100);
        invoice.TotalAmount = invoice.SubTotal + invoice.TaxAmount - invoice.DiscountAmount;
        invoice.UpdatedAt = DateTime.UtcNow;
        invoice.UpdatedBy = currentUser;

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
