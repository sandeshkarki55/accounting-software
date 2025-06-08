using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;

namespace AccountingApi.Features.Customers;

// Query to get a customer by ID
public record GetCustomerByIdQuery(int Id) : IRequest<CustomerDto?>;

// Handler for GetCustomerByIdQuery
public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
{
    private readonly AccountingDbContext _context;
    private readonly CustomerMapper _customerMapper;

    public GetCustomerByIdQueryHandler(AccountingDbContext context, CustomerMapper customerMapper)
    {
        _context = context;
        _customerMapper = customerMapper;
    }

    public async Task<CustomerDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer == null)
            return null;

        return _customerMapper.ToDto(customer);
    }
}