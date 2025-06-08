using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Models;

namespace AccountingApi.Features.CompanyInfo;

// Command to create company info
public record CreateCompanyInfoCommand(CreateCompanyInfoDto CompanyInfo) : IRequest<CompanyInfoDto>;

// Handler for CreateCompanyInfoCommand
public class CreateCompanyInfoCommandHandler : IRequestHandler<CreateCompanyInfoCommand, CompanyInfoDto>
{
    private readonly AccountingDbContext _context;

    public CreateCompanyInfoCommandHandler(AccountingDbContext context)
    {
        _context = context;
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

        var companyInfo = new Models.CompanyInfo
        {
            CompanyName = request.CompanyInfo.CompanyName,
            LegalName = request.CompanyInfo.LegalName,
            TaxNumber = request.CompanyInfo.TaxNumber,
            RegistrationNumber = request.CompanyInfo.RegistrationNumber,
            Email = request.CompanyInfo.Email,
            Phone = request.CompanyInfo.Phone,
            Website = request.CompanyInfo.Website,
            Address = request.CompanyInfo.Address,
            City = request.CompanyInfo.City,
            State = request.CompanyInfo.State,
            PostalCode = request.CompanyInfo.PostalCode,
            Country = request.CompanyInfo.Country,
            LogoUrl = request.CompanyInfo.LogoUrl,
            BankName = request.CompanyInfo.BankName,
            BankAccountNumber = request.CompanyInfo.BankAccountNumber,
            BankRoutingNumber = request.CompanyInfo.BankRoutingNumber,
            Currency = request.CompanyInfo.Currency,
            IsDefault = request.CompanyInfo.IsDefault,
            CreatedBy = "System", // TODO: Replace with actual user when authentication is implemented
            UpdatedBy = "System"
        };

        _context.CompanyInfos.Add(companyInfo);
        await _context.SaveChangesAsync(cancellationToken);

        return new CompanyInfoDto
        {
            Id = companyInfo.Id,
            CompanyName = companyInfo.CompanyName,
            LegalName = companyInfo.LegalName,
            TaxNumber = companyInfo.TaxNumber,
            RegistrationNumber = companyInfo.RegistrationNumber,
            Email = companyInfo.Email,
            Phone = companyInfo.Phone,
            Website = companyInfo.Website,
            Address = companyInfo.Address,
            City = companyInfo.City,
            State = companyInfo.State,
            PostalCode = companyInfo.PostalCode,
            Country = companyInfo.Country,
            LogoUrl = companyInfo.LogoUrl,
            BankName = companyInfo.BankName,
            BankAccountNumber = companyInfo.BankAccountNumber,
            BankRoutingNumber = companyInfo.BankRoutingNumber,
            Currency = companyInfo.Currency,
            IsDefault = companyInfo.IsDefault
        };
    }
}