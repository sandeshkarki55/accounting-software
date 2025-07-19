using MyMediator;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Services.CurrentUserService;
using AccountingApi.Mappings;

namespace AccountingApi.Features.Customers;

// Command to update a customer
public record UpdateCustomerCommand(int Id, UpdateCustomerDto Customer) : IRequest<CustomerDto?>;

// Handler for UpdateCustomerCommand
public class UpdateCustomerCommandHandler(
    AccountingDbContext context, 
    ICurrentUserService currentUserService,
    CustomerMapper customerMapper) 
    : IRequestHandler<UpdateCustomerCommand, CustomerDto?>
{
    public async Task<CustomerDto?> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer == null)
            return null;

        customerMapper.UpdateEntity(customer, request.Customer);
        customer.UpdatedBy = currentUserService.GetCurrentUserForAudit();

        await context.SaveChangesAsync(cancellationToken);

        return customerMapper.ToDto(customer);
    }
}