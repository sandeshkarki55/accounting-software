using AccountingApi.DTOs;
using AccountingApi.Models;

namespace AccountingApi.Mappings;

/// <summary>
/// Mapper for Customer entity and related DTOs
/// </summary>
public class CustomerMapper : IEntityMapper<Customer, CustomerDto, CreateCustomerDto, UpdateCustomerDto>
{
    /// <summary>
    /// Maps a Customer entity to CustomerDto
    /// </summary>
    /// <param name="entity">The Customer entity to map</param>
    /// <returns>The mapped CustomerDto</returns>
    public CustomerDto ToDto(Customer entity)
    {
        return new CustomerDto
        {
            Id = entity.Id,
            CustomerCode = entity.CustomerCode,
            CompanyName = entity.CompanyName,
            ContactPersonName = entity.ContactPersonName,
            Email = entity.Email,
            Phone = entity.Phone,
            Address = entity.Address,
            City = entity.City,
            State = entity.State,
            PostalCode = entity.PostalCode,
            Country = entity.Country,
            IsActive = entity.IsActive,
            Notes = entity.Notes
        };
    }

    /// <summary>
    /// Maps a collection of Customer entities to CustomerDto collection
    /// </summary>
    /// <param name="entities">The Customer entities to map</param>
    /// <returns>The mapped CustomerDto collection</returns>
    public IEnumerable<CustomerDto> ToDto(IEnumerable<Customer> entities)
    {
        return entities.Select(ToDto);
    }

    /// <summary>
    /// Maps a CreateCustomerDto to Customer entity
    /// </summary>
    /// <param name="createDto">The CreateCustomerDto to map</param>
    /// <returns>The mapped Customer entity</returns>
    public Customer ToEntity(CreateCustomerDto createDto)
    {
        return new Customer
        {
            // CustomerCode will be set by the service layer using auto-generation
            CompanyName = createDto.CompanyName,
            ContactPersonName = createDto.ContactPersonName,
            Email = createDto.Email,
            Phone = createDto.Phone,
            Address = createDto.Address,
            City = createDto.City,
            State = createDto.State,
            PostalCode = createDto.PostalCode,
            Country = createDto.Country,
            IsActive = true,
            Notes = createDto.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates an existing Customer entity with data from UpdateCustomerDto
    /// </summary>
    /// <param name="entity">The Customer entity to update</param>
    /// <param name="updateDto">The UpdateCustomerDto containing new data</param>
    public void UpdateEntity(Customer entity, UpdateCustomerDto updateDto)
    {
        entity.CompanyName = updateDto.CompanyName;
        entity.ContactPersonName = updateDto.ContactPersonName;
        entity.Email = updateDto.Email;
        entity.Phone = updateDto.Phone;
        entity.Address = updateDto.Address;
        entity.City = updateDto.City;
        entity.State = updateDto.State;
        entity.PostalCode = updateDto.PostalCode;
        entity.Country = updateDto.Country;
        entity.IsActive = updateDto.IsActive;
        entity.Notes = updateDto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}