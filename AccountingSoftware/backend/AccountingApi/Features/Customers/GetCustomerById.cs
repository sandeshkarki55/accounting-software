using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;

namespace AccountingApi.Features.Customers;

// Query to get a customer by ID
public record GetCustomerByIdQuery(int Id) : IRequest<CustomerDto?>;

// Handler for GetCustomerByIdQuery
public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
{
    private readonly AccountingDbContext _context;

    public GetCustomerByIdQueryHandler(AccountingDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .Where(c => c.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);

        return customer;
    }
}