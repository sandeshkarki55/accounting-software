using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Models;
using AccountingApi.Mappings;

namespace AccountingApi.Features.CompanyInfo;

// Command to create company info
public record CreateCompanyInfoCommand(CreateCompanyInfoDto CompanyInfo) : IRequest<CompanyInfoDto>;

// Handler for CreateCompanyInfoCommand
public class CreateCompanyInfoCommandHandler : IRequestHandler<CreateCompanyInfoCommand, CompanyInfoDto>
{
    private readonly AccountingDbContext _context;
    private readonly CompanyInfoMapper _mapper;

    public CreateCompanyInfoCommandHandler(AccountingDbContext context, CompanyInfoMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CompanyInfoDto> Handle(CreateCompanyInfoCommand request, CancellationToken cancellationToken)
    {
        // If this is being set as default, unset all other defaults
        if (request.CompanyInfo.IsDefault)
        {
            var existingDefaults = await _context.CompanyInfos
                .Where(c => c.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var existing in existingDefaults)
            {
                existing.IsDefault = false;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = "System";
            }
        }

        var companyInfo = _mapper.ToEntity(request.CompanyInfo);
        companyInfo.CreatedBy = "System"; // TODO: Replace with actual user when authentication is implemented
        companyInfo.UpdatedBy = "System";

        _context.CompanyInfos.Add(companyInfo);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.ToDto(companyInfo);
    }
}