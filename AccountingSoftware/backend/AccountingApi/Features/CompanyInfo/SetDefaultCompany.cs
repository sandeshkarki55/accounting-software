using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;

using Microsoft.EntityFrameworkCore;

using MyMediator;

namespace AccountingApi.Features.CompanyInfo;

public record SetDefaultCompanyCommand(int Id) : IRequest<CompanyInfoDto>;

public class SetDefaultCompanyHandler(AccountingDbContext context, CompanyInfoMapper mapper) : IRequestHandler<SetDefaultCompanyCommand, CompanyInfoDto>
{
    public async Task<CompanyInfoDto> Handle(SetDefaultCompanyCommand request, CancellationToken cancellationToken)
    {
        var companyInfo = await context.CompanyInfos
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (companyInfo == null)
        {
            throw new KeyNotFoundException($"Company with ID {request.Id} not found");
        }

        // Set all other companies to not default
        var otherCompanies = await context.CompanyInfos
            .Where(c => c.Id != request.Id && c.IsDefault)
            .ToListAsync(cancellationToken);

        foreach (var other in otherCompanies)
        {
            other.IsDefault = false;
        }

        // Set this company as default
        companyInfo.IsDefault = true;

        await context.SaveChangesAsync(cancellationToken);

        return mapper.ToDto(companyInfo);
    }
}