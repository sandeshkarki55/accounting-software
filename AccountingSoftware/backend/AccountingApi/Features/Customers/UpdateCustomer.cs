using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Services;

namespace AccountingApi.Features.Customers;

// Command to update a customer
public record UpdateCustomerCommand(int Id, UpdateCustomerDto Customer) : IRequest<CustomerDto?>;

// Handler for UpdateCustomerCommand
public class UpdateCustomerCommandHandler(AccountingDbContext context, ICurrentUserService currentUserService) : IRequestHandler<UpdateCustomerCommand, CustomerDto?>
{
    public async Task<CustomerDto?> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer == null)
            return null;

        customer.CompanyName = request.Customer.CompanyName;
        customer.ContactPersonName = request.Customer.ContactPersonName;
        customer.Email = request.Customer.Email;
        customer.Phone = request.Customer.Phone;
        customer.Address = request.Customer.Address;
        customer.City = request.Customer.City;
        customer.State = request.Customer.State;
        customer.PostalCode = request.Customer.PostalCode;
        customer.Country = request.Customer.Country;
        customer.IsActive = request.Customer.IsActive;
        customer.Notes = request.Customer.Notes;
        customer.UpdatedAt = DateTime.UtcNow;
        customer.UpdatedBy = currentUserService.GetCurrentUserForAudit();

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