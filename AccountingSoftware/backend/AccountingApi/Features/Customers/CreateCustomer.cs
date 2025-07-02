using MediatR;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Models;
using AccountingApi.Services;

namespace AccountingApi.Features.Customers;

// Command to create a customer
public record CreateCustomerCommand(CreateCustomerDto Customer) : IRequest<CustomerDto>;

// Handler for CreateCustomerCommand
public class CreateCustomerCommandHandler(AccountingDbContext context, INumberGenerationService numberGenerationService) : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        // Generate customer code
        var customerCode = await numberGenerationService.GenerateCustomerCodeAsync();

        var customer = new Customer
        {
            CustomerCode = customerCode,
            CompanyName = request.Customer.CompanyName,
            ContactPersonName = request.Customer.ContactPersonName,
            Email = request.Customer.Email,
            Phone = request.Customer.Phone,
            Address = request.Customer.Address,
            City = request.Customer.City,
            State = request.Customer.State,
            PostalCode = request.Customer.PostalCode,
            Country = request.Customer.Country,
            Notes = request.Customer.Notes,
            CreatedBy = "System", // TODO: Replace with actual user when authentication is implemented
            UpdatedBy = "System"
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync(cancellationToken);

        return new CustomerDto
        {
            Id = customer.Id,
            CustomerCode = customer.CustomerCode,
            CompanyName = customer.CompanyName,
            ContactPersonName = customer.ContactPersonName,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            City = customer.City,
            State = customer.State,
            PostalCode = customer.PostalCode,
            Country = customer.Country,
            IsActive = customer.IsActive,
            Notes = customer.Notes
        };
    }
}