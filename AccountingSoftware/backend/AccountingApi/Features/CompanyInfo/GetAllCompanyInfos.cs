using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;

namespace AccountingApi.Features.CompanyInfo;

// Query to get all company info records
public record GetAllCompanyInfosQuery : IRequest<List<CompanyInfoDto>>;

// Handler for GetAllCompanyInfosQuery
public class GetAllCompanyInfosQueryHandler : IRequestHandler<GetAllCompanyInfosQuery, List<CompanyInfoDto>>
{
    private readonly AccountingDbContext _context;
    private readonly CompanyInfoMapper _mapper;

    public GetAllCompanyInfosQueryHandler(AccountingDbContext context, CompanyInfoMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<CompanyInfoDto>> Handle(GetAllCompanyInfosQuery request, CancellationToken cancellationToken)
    {
        var companyInfos = await _context.CompanyInfos
            .OrderByDescending(c => c.IsDefault)
            .ThenBy(c => c.CompanyName)
            .ToListAsync(cancellationToken);

        return _mapper.ToDto(companyInfos).ToList();
    }
}