using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.Infrastructure;
using AccountingApi.Models;

namespace AccountingApi.Features.Invoices;

// Command to delete an invoice (soft delete)
public record DeleteInvoiceCommand(int Id) : IRequest<bool>;

// Handler for DeleteInvoiceCommand
public class DeleteInvoiceCommandHandler(AccountingDbContext context) : IRequestHandler<DeleteInvoiceCommand, bool>
{
    public async Task<bool> Handle(DeleteInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (invoice == null)
            return false;

        // Business rule: Cannot delete paid invoices
        if (invoice.Status == InvoiceStatus.Paid)
        {
            throw new InvalidOperationException("Cannot delete a paid invoice. Paid invoices can only be archived through other means.");
        }

        // Perform soft delete on invoice and its items
        invoice.IsDeleted = true;
        invoice.DeletedAt = DateTime.UtcNow;
        invoice.DeletedBy = "System"; // TODO: Replace with actual user when authentication is implemented
        invoice.UpdatedAt = DateTime.UtcNow;
        invoice.UpdatedBy = "System";

        // Also soft delete all invoice items
        foreach (var item in invoice.Items)
        {
            item.IsDeleted = true;
            item.DeletedAt = DateTime.UtcNow;
            item.DeletedBy = "System";
            item.UpdatedAt = DateTime.UtcNow;
            item.UpdatedBy = "System";
        }

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
