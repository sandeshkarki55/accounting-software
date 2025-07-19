using MyMediator;
using Microsoft.EntityFrameworkCore;
using AccountingApi.Infrastructure;
using AccountingApi.Models;
using AccountingApi.Services.CurrentUserService;

namespace AccountingApi.Features.Invoices;

// Command to delete an invoice (soft delete)
public record DeleteInvoiceCommand(int Id) : IRequest<bool>;

// Handler for DeleteInvoiceCommand
public class DeleteInvoiceCommandHandler(AccountingDbContext context, ICurrentUserService currentUserService) : IRequestHandler<DeleteInvoiceCommand, bool>
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

        var currentUser = currentUserService.GetCurrentUserForAudit();

        // Perform soft delete on invoice and its items
        invoice.IsDeleted = true;
        invoice.DeletedAt = DateTime.UtcNow;
        invoice.DeletedBy = currentUser;
        invoice.UpdatedAt = DateTime.UtcNow;
        invoice.UpdatedBy = currentUser;

        // Also soft delete all invoice items
        foreach (var item in invoice.Items)
        {
            item.IsDeleted = true;
            item.DeletedAt = DateTime.UtcNow;
            item.DeletedBy = currentUser;
            item.UpdatedAt = DateTime.UtcNow;
            item.UpdatedBy = currentUser;
        }

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
