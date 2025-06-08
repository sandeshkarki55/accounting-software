using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;

namespace AccountingApi.Features.CompanyInfo;

// Query to get all company info records
public record GetAllCompanyInfosQuery : IRequest<List<CompanyInfoDto>>;

// Handler for GetAllCompanyInfosQuery
public class GetAllCompanyInfosQueryHandler : IRequestHandler<GetAllCompanyInfosQuery, List<CompanyInfoDto>>
{
    private readonly AccountingDbContext _context;

    public GetAllCompanyInfosQueryHandler(AccountingDbContext context)
    {
        _context = context;
    }

    public async Task<List<CompanyInfoDto>> Handle(GetAllCompanyInfosQuery request, CancellationToken cancellationToken)
    {
        var companyInfos = await _context.CompanyInfos
            .OrderByDescending(c => c.IsDefault)
            .ThenBy(c => c.CompanyName)
            .Select(c => new CompanyInfoDto
            {
                Id = c.Id,
                CompanyName = c.CompanyName,
                LegalName = c.LegalName,
                TaxNumber = c.TaxNumber,
                RegistrationNumber = c.RegistrationNumber,
                Email = c.Email,
                Phone = c.Phone,
                Website = c.Website,
                Address = c.Address,
                City = c.City,
                State = c.State,
                PostalCode = c.PostalCode,
                Country = c.Country,
                LogoUrl = c.LogoUrl,
                BankName = c.BankName,
                BankAccountNumber = c.BankAccountNumber,
                BankRoutingNumber = c.BankRoutingNumber,
                Currency = c.Currency,
                IsDefault = c.IsDefault
            })
            .ToListAsync(cancellationToken);

        return companyInfos;
    }
}