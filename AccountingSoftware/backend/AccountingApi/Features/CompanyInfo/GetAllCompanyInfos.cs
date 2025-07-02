using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;

namespace AccountingApi.Features.CompanyInfo;

// Query to get all company info records
public record GetAllCompanyInfosQuery : IRequest<List<CompanyInfoDto>>;

// Handler for GetAllCompanyInfosQuery
public class GetAllCompanyInfosQueryHandler(AccountingDbContext context, CompanyInfoMapper mapper) : IRequestHandler<GetAllCompanyInfosQuery, List<CompanyInfoDto>>
{
    public async Task<List<CompanyInfoDto>> Handle(GetAllCompanyInfosQuery request, CancellationToken cancellationToken)
    {
        var companyInfos = await context.CompanyInfos
            .OrderByDescending(c => c.IsDefault)
            .ThenBy(c => c.CompanyName)
            .ToListAsync(cancellationToken);

        return mapper.ToDto(companyInfos).ToList();
    }
}