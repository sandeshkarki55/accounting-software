using MediatR;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Models;

namespace AccountingApi.Features.Customers;

// Command to create a customer
public record CreateCustomerCommand(CreateCustomerDto Customer) : IRequest<CustomerDto>;

// Handler for CreateCustomerCommand
public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly AccountingDbContext _context;

    public CreateCustomerCommandHandler(AccountingDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = new Customer
        {
            CustomerCode = request.Customer.CustomerCode,
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

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);

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