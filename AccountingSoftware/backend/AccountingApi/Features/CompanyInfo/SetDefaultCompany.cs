using MediatR;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;
using Microsoft.EntityFrameworkCore;

namespace AccountingApi.Features.CompanyInfo;

public record SetDefaultCompanyCommand(int Id) : IRequest<CompanyInfoDto>;

public class SetDefaultCompanyHandler : IRequestHandler<SetDefaultCompanyCommand, CompanyInfoDto>
{
    private readonly AccountingDbContext _context;
    private readonly CompanyInfoMapper _mapper;

    public SetDefaultCompanyHandler(AccountingDbContext context, CompanyInfoMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CompanyInfoDto> Handle(SetDefaultCompanyCommand request, CancellationToken cancellationToken)
    {
        var companyInfo = await _context.CompanyInfos
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (companyInfo == null)
        {
            throw new KeyNotFoundException($"Company with ID {request.Id} not found");
        }

        // Set all other companies to not default
        var otherCompanies = await _context.CompanyInfos
            .Where(c => c.Id != request.Id && c.IsDefault)
            .ToListAsync(cancellationToken);

        foreach (var other in otherCompanies)
        {
            other.IsDefault = false;
        }

        // Set this company as default
        companyInfo.IsDefault = true;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.ToDto(companyInfo);
    }
}
