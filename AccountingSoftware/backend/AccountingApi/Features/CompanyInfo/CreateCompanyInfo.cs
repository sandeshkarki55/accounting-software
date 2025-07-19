using MyMediator;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;
using AccountingApi.Services.CurrentUserService;

namespace AccountingApi.Features.CompanyInfo;

// Command to create company info
public record CreateCompanyInfoCommand(CreateCompanyInfoDto CompanyInfo) : IRequest<CompanyInfoDto>;

// Handler for CreateCompanyInfoCommand
public class CreateCompanyInfoCommandHandler(AccountingDbContext context, CompanyInfoMapper mapper, ICurrentUserService currentUserService) : IRequestHandler<CreateCompanyInfoCommand, CompanyInfoDto>
{
    public async Task<CompanyInfoDto> Handle(CreateCompanyInfoCommand request, CancellationToken cancellationToken)
    {
        var currentUser = currentUserService.GetCurrentUserForAudit();
        
        // If this is being set as default, unset all other defaults
        if (request.CompanyInfo.IsDefault)
        {
            var existingDefaults = await context.CompanyInfos
                .Where(c => c.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var existing in existingDefaults)
            {
                existing.IsDefault = false;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = currentUser;
            }
        }

        var companyInfo = mapper.ToEntity(request.CompanyInfo);
        companyInfo.CreatedBy = currentUser;
        companyInfo.UpdatedBy = currentUser;

        context.CompanyInfos.Add(companyInfo);
        await context.SaveChangesAsync(cancellationToken);

        return mapper.ToDto(companyInfo);
    }
}