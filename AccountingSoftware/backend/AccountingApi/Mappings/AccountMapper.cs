using AccountingApi.DTOs;
using AccountingApi.Models;

namespace AccountingApi.Mappings;

/// <summary>
/// Mapper for Account entity and related DTOs
/// </summary>
public class AccountMapper : IEntityMapper<Account, AccountDto, CreateAccountDto, UpdateAccountDto>
{
    /// <summary>
    /// Maps an Account entity to AccountDto
    /// </summary>
    /// <param name="entity">The Account entity to map</param>
    /// <returns>The mapped AccountDto</returns>
    public AccountDto ToDto(Account entity)
    {
        return new AccountDto
        {
            Id = entity.Id,
            AccountCode = entity.AccountCode,
            AccountName = entity.AccountName,
            AccountType = entity.AccountType,
            Balance = entity.Balance,
            IsActive = entity.IsActive,
            Description = entity.Description,
            ParentAccountId = entity.ParentAccountId,
            ParentAccountName = entity.ParentAccount?.AccountName,
            Level = CalculateLevel(entity),
            SubAccounts = entity.SubAccounts?.Select(ToDto).ToList() ?? []
        };
    }

    /// <summary>
    /// Maps a collection of Account entities to AccountDto collection
    /// </summary>
    /// <param name="entities">The Account entities to map</param>
    /// <returns>The mapped AccountDto collection</returns>
    public IEnumerable<AccountDto> ToDto(IEnumerable<Account> entities)
    {
        return entities.Select(ToDto);
    }

    /// <summary>
    /// Maps a CreateAccountDto to Account entity
    /// </summary>
    /// <param name="createDto">The CreateAccountDto to map</param>
    /// <returns>The mapped Account entity</returns>
    public Account ToEntity(CreateAccountDto createDto)
    {
        return new Account
        {
            AccountCode = createDto.AccountCode,
            AccountName = createDto.AccountName,
            AccountType = createDto.AccountType,
            Description = createDto.Description,
            ParentAccountId = createDto.ParentAccountId,
            Balance = 0, // New accounts start with zero balance
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates an existing Account entity with data from UpdateAccountDto
    /// </summary>
    /// <param name="entity">The Account entity to update</param>
    /// <param name="updateDto">The UpdateAccountDto containing new data</param>
    public void UpdateEntity(Account entity, UpdateAccountDto updateDto)
    {
        entity.AccountName = updateDto.AccountName;
        entity.Description = updateDto.Description;
        entity.IsActive = updateDto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Calculates the hierarchical level of an account
    /// </summary>
    /// <param name="account">The account to calculate level for</param>
    /// <returns>The hierarchical level (0 for root accounts)</returns>
    private static int CalculateLevel(Account account)
    {
        int level = 0;
        var current = account.ParentAccount;

        while (current != null)
        {
            level++;
            current = current.ParentAccount;
        }

        return level;
    }
}