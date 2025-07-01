using MediatR;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;
using Microsoft.EntityFrameworkCore;

namespace AccountingApi.Features.CompanyInfo;

public record UpdateCompanyInfoCommand(int Id, CreateCompanyInfoDto CompanyInfo) : IRequest<CompanyInfoDto>;

public class UpdateCompanyInfoHandler : IRequestHandler<UpdateCompanyInfoCommand, CompanyInfoDto>
{
    private readonly AccountingDbContext _context;
    private readonly CompanyInfoMapper _mapper;

    public UpdateCompanyInfoHandler(AccountingDbContext context, CompanyInfoMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CompanyInfoDto> Handle(UpdateCompanyInfoCommand request, CancellationToken cancellationToken)
    {
        var companyInfo = await _context.CompanyInfos
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (companyInfo == null)
        {
            throw new KeyNotFoundException($"Company with ID {request.Id} not found");
        }

        // Handle isDefault logic
        if (request.CompanyInfo.IsDefault && !companyInfo.IsDefault)
        {
            // Set all other companies to not default
            var otherCompanies = await _context.CompanyInfos
                .Where(c => c.Id != request.Id && c.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var other in otherCompanies)
            {
                other.IsDefault = false;
            }
        }

        // Update the entity using the mapper
        _mapper.UpdateEntity(companyInfo, request.CompanyInfo);

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.ToDto(companyInfo);
    }
}
