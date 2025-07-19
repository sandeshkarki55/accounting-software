using MyMediator;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;
using Microsoft.EntityFrameworkCore;

namespace AccountingApi.Features.CompanyInfo;

public record UpdateCompanyInfoCommand(int Id, CreateCompanyInfoDto CompanyInfo) : IRequest<CompanyInfoDto>;

public class UpdateCompanyInfoHandler(AccountingDbContext context, CompanyInfoMapper mapper) : IRequestHandler<UpdateCompanyInfoCommand, CompanyInfoDto>
{
    public async Task<CompanyInfoDto> Handle(UpdateCompanyInfoCommand request, CancellationToken cancellationToken)
    {
        var companyInfo = await context.CompanyInfos
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (companyInfo == null)
        {
            throw new KeyNotFoundException($"Company with ID {request.Id} not found");
        }

        // Handle isDefault logic
        if (request.CompanyInfo.IsDefault && !companyInfo.IsDefault)
        {
            // Set all other companies to not default
            var otherCompanies = await context.CompanyInfos
                .Where(c => c.Id != request.Id && c.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var other in otherCompanies)
            {
                other.IsDefault = false;
            }
        }

        // Update the entity using the mapper
        mapper.UpdateEntity(companyInfo, request.CompanyInfo);

        await context.SaveChangesAsync(cancellationToken);

        return mapper.ToDto(companyInfo);
    }
}
