using MediatR;
using AccountingApi.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AccountingApi.Features.CompanyInfo;

public record DeleteCompanyInfoCommand(int Id) : IRequest;

public class DeleteCompanyInfoHandler : IRequestHandler<DeleteCompanyInfoCommand>
{
    private readonly AccountingDbContext _context;

    public DeleteCompanyInfoHandler(AccountingDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteCompanyInfoCommand request, CancellationToken cancellationToken)
    {
        var companyInfo = await _context.CompanyInfos
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (companyInfo == null)
        {
            throw new KeyNotFoundException($"Company with ID {request.Id} not found");
        }

        // Prevent deletion of default company
        if (companyInfo.IsDefault)
        {
            throw new InvalidOperationException("Cannot delete the default company. Please set another company as default first.");
        }

        _context.CompanyInfos.Remove(companyInfo);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
