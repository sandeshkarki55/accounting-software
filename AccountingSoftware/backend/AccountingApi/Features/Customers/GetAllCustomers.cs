using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;
using AccountingApi.Models;

namespace AccountingApi.Features.Customers;

// Query to get all customers
public record GetAllCustomersQuery : IRequest<List<CustomerDto>>;

// Handler for GetAllCustomersQuery
public class GetAllCustomersQueryHandler(
    AccountingDbContext context,
    CustomerMapper customerMapper) 
    : IRequestHandler<GetAllCustomersQuery, List<CustomerDto>>
{
    public async Task<List<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await context.Customers
            .Where(c => c.IsActive)
            .OrderBy(c => c.CompanyName)
            .ToListAsync(cancellationToken);

        return customerMapper.ToDto(customers).ToList();
    }
}