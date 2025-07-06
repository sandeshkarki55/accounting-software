using MediatR;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Models;
using AccountingApi.Services;
using AccountingApi.Mappings;

namespace AccountingApi.Features.Customers;

// Command to create a customer
public record CreateCustomerCommand(CreateCustomerDto Customer) : IRequest<CustomerDto>;

// Handler for CreateCustomerCommand
public class CreateCustomerCommandHandler(
    AccountingDbContext context, 
    INumberGenerationService numberGenerationService, 
    ICurrentUserService currentUserService,
    CustomerMapper customerMapper) 
    : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        // Generate customer code
        var customerCode = await numberGenerationService.GenerateCustomerCodeAsync();
        var currentUser = currentUserService.GetCurrentUserForAudit();

        var customer = customerMapper.ToEntity(request.Customer);
        customer.CustomerCode = customerCode;
        customer.CreatedBy = currentUser;
        customer.UpdatedBy = currentUser;

        context.Customers.Add(customer);
        await context.SaveChangesAsync(cancellationToken);

        return customerMapper.ToDto(customer);
    }
}