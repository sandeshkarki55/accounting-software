using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.Infrastructure;

namespace AccountingApi.Features.Customers;

// Command to delete a customer (soft delete)
public record DeleteCustomerCommand(int Id) : IRequest<bool>;

// Handler for DeleteCustomerCommand
public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, bool>
{
    private readonly AccountingDbContext _context;

    public DeleteCustomerCommandHandler(AccountingDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
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

        // Perform soft delete
        customer.IsDeleted = true;
        customer.DeletedAt = DateTime.UtcNow;
        customer.DeletedBy = "System"; // TODO: Replace with actual user when authentication is implemented
        customer.UpdatedAt = DateTime.UtcNow;
        customer.UpdatedBy = "System";

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
