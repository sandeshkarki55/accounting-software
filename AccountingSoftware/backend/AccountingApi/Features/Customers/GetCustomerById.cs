using MyMediator;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;

namespace AccountingApi.Features.Customers;

// Query to get a customer by ID
public record GetCustomerByIdQuery(int Id) : IRequest<CustomerDto?>;

// Handler for GetCustomerByIdQuery
public class GetCustomerByIdQueryHandler(
    AccountingDbContext context, 
    CustomerMapper customerMapper) 
    : IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
{
    public async Task<CustomerDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer == null)
            return null;

        return customerMapper.ToDto(customer);
    }
}