using AccountingApi.Infrastructure;
using AccountingApi.Services.CurrentUserService;

using Microsoft.EntityFrameworkCore;

using MyMediator;

namespace AccountingApi.Features.Customers;

// Command to delete a customer (soft delete)
public record DeleteCustomerCommand(int Id) : IRequest<bool>;

// Handler for DeleteCustomerCommand
public class DeleteCustomerCommandHandler(AccountingDbContext context, ICurrentUserService currentUserService) : IRequestHandler<DeleteCustomerCommand, bool>
{
    public async Task<bool> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await context.Customers
            .Include(c => c.Invoices)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer == null)
            return false;

        // Business rule: Check if customer has any non-deleted invoices
        var hasActiveInvoices = customer.Invoices.Any(i => !i.IsDeleted);
        if (hasActiveInvoices)
        {
            throw new InvalidOperationException("Cannot delete customer that has active invoices. Please delete or archive the invoices first.");
        }

        var currentUser = currentUserService.GetCurrentUserForAudit();

        // Perform soft delete
        customer.IsDeleted = true;
        customer.DeletedAt = DateTime.UtcNow;
        customer.DeletedBy = currentUser;
        customer.UpdatedAt = DateTime.UtcNow;
        customer.UpdatedBy = currentUser;

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}