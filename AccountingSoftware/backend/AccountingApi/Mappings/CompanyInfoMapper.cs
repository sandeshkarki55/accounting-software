using AccountingApi.Models;
using AccountingApi.DTOs;

namespace AccountingApi.Mappings;

/// <summary>
/// Mapper for CompanyInfo entity and related DTOs
/// </summary>
public class CompanyInfoMapper : IEntityMapper<CompanyInfo, CompanyInfoDto, CreateCompanyInfoDto, CreateCompanyInfoDto>
{
    /// <summary>
    /// Maps a CompanyInfo entity to CompanyInfoDto
    /// </summary>
    /// <param name="entity">The CompanyInfo entity to map</param>
    /// <returns>The mapped CompanyInfoDto</returns>
    public CompanyInfoDto ToDto(CompanyInfo entity)
    {
        return new CompanyInfoDto
        {
            Id = entity.Id,
            CompanyName = entity.CompanyName,
            LegalName = entity.LegalName,
            TaxNumber = entity.TaxNumber,
            RegistrationNumber = entity.RegistrationNumber,
            Email = entity.Email,
            Phone = entity.Phone,
            Website = entity.Website,
            Address = entity.Address,
            City = entity.City,
            State = entity.State,
            PostalCode = entity.PostalCode,
            Country = entity.Country,
            LogoUrl = entity.LogoUrl,
            BankName = entity.BankName,
            BankAccountNumber = entity.BankAccountNumber,
            BankRoutingNumber = entity.BankRoutingNumber,
            Currency = entity.Currency,
            IsDefault = entity.IsDefault
        };
    }

    /// <summary>
    /// Maps a collection of CompanyInfo entities to CompanyInfoDto collection
    /// </summary>
    /// <param name="entities">The CompanyInfo entities to map</param>
    /// <returns>The mapped CompanyInfoDto collection</returns>
    public IEnumerable<CompanyInfoDto> ToDto(IEnumerable<CompanyInfo> entities)
    {
        return entities.Select(ToDto);
    }

    /// <summary>
    /// Maps a CreateCompanyInfoDto to CompanyInfo entity
    /// </summary>
    /// <param name="createDto">The CreateCompanyInfoDto to map</param>
    /// <returns>The mapped CompanyInfo entity</returns>
    public CompanyInfo ToEntity(CreateCompanyInfoDto createDto)
    {
        return new CompanyInfo
        {
            CompanyName = createDto.CompanyName,
            LegalName = createDto.LegalName,
            TaxNumber = createDto.TaxNumber,
            RegistrationNumber = createDto.RegistrationNumber,
            Email = createDto.Email,
            Phone = createDto.Phone,
            Website = createDto.Website,
            Address = createDto.Address,
            City = createDto.City,
            State = createDto.State,
            PostalCode = createDto.PostalCode,
            Country = createDto.Country,
            LogoUrl = createDto.LogoUrl,
            BankName = createDto.BankName,
            BankAccountNumber = createDto.BankAccountNumber,
            BankRoutingNumber = createDto.BankRoutingNumber,
            Currency = createDto.Currency,
            IsDefault = createDto.IsDefault,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates an existing CompanyInfo entity with data from CreateCompanyInfoDto
    /// </summary>
    /// <param name="entity">The CompanyInfo entity to update</param>
    /// <param name="updateDto">The CreateCompanyInfoDto containing new data</param>
    public void UpdateEntity(CompanyInfo entity, CreateCompanyInfoDto updateDto)
    {
        entity.CompanyName = updateDto.CompanyName;
        entity.LegalName = updateDto.LegalName;
        entity.TaxNumber = updateDto.TaxNumber;
        entity.RegistrationNumber = updateDto.RegistrationNumber;
        entity.Email = updateDto.Email;
        entity.Phone = updateDto.Phone;
        entity.Website = updateDto.Website;
        entity.Address = updateDto.Address;
        entity.City = updateDto.City;
        entity.State = updateDto.State;
        entity.PostalCode = updateDto.PostalCode;
        entity.Country = updateDto.Country;
        entity.LogoUrl = updateDto.LogoUrl;
        entity.BankName = updateDto.BankName;
        entity.BankAccountNumber = updateDto.BankAccountNumber;
        entity.BankRoutingNumber = updateDto.BankRoutingNumber;
        entity.Currency = updateDto.Currency;
        entity.IsDefault = updateDto.IsDefault;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}