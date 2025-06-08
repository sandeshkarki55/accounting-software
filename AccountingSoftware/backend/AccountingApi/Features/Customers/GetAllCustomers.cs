using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;

namespace AccountingApi.Features.Customers;

// Query to get all customers
public record GetAllCustomersQuery : IRequest<List<CustomerDto>>;

// Handler for GetAllCustomersQuery
public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, List<CustomerDto>>
{
    private readonly AccountingDbContext _context;

    public GetAllCustomersQueryHandler(AccountingDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await _context.Customers
            .Where(c => c.IsActive)
            .OrderBy(c => c.CompanyName)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                CustomerCode = c.CustomerCode,
                CompanyName = c.CompanyName,
                ContactPersonName = c.ContactPersonName,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                City = c.City,
                State = c.State,
                PostalCode = c.PostalCode,
                Country = c.Country,
                IsActive = c.IsActive,
                Notes = c.Notes
            })
            .ToListAsync(cancellationToken);

        return customers;
    }
}