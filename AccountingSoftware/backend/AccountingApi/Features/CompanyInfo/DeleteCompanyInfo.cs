using MyMediator;
using AccountingApi.Infrastructure;
using Microsoft.EntityFrameworkCore;
using AccountingApi.Services;

namespace AccountingApi.Features.CompanyInfo;

// Command to delete company info (soft delete)
public record DeleteCompanyInfoCommand(int Id) : IRequest<bool>;

public class DeleteCompanyInfoHandler(AccountingDbContext context, ICurrentUserService currentUserService) : IRequestHandler<DeleteCompanyInfoCommand, bool>
{
    public async Task<bool> Handle(DeleteCompanyInfoCommand request, CancellationToken cancellationToken)
    {
        var companyInfo = await context.CompanyInfos
            .Include(c => c.Invoices)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (companyInfo == null)
            return false;

        // Prevent deletion of default company
        if (companyInfo.IsDefault)
        {
            throw new InvalidOperationException("Cannot delete the default company. Please set another company as default first.");
        }

        // Check if company has any non-deleted invoices
        var hasActiveInvoices = companyInfo.Invoices.Any(i => !i.IsDeleted);
        if (hasActiveInvoices)
        {
            throw new InvalidOperationException("Cannot delete company that has active invoices. Please delete or archive the invoices first.");
        }

        var currentUser = currentUserService.GetCurrentUserForAudit();

        // Perform soft delete
        companyInfo.IsDeleted = true;
        companyInfo.DeletedAt = DateTime.UtcNow;
        companyInfo.DeletedBy = currentUser;
        companyInfo.UpdatedAt = DateTime.UtcNow;
        companyInfo.UpdatedBy = currentUser;

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
