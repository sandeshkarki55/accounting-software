using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;

namespace AccountingApi.Features.Accounts;

// Query for GetAccountsHierarchy
using MyMediator;
public record GetAccountsHierarchyQuery : IRequest<List<AccountDto>>;

// Handler for GetAccountsHierarchyQuery
public class GetAccountsHierarchyQueryHandler(AccountingDbContext context) : IRequestHandler<GetAccountsHierarchyQuery, List<AccountDto>>
{
    public async Task<List<AccountDto>> Handle(GetAccountsHierarchyQuery request, CancellationToken cancellationToken)
    {
        // Get all accounts with their parent relationships
        var allAccounts = await context.Accounts
            .Include(a => a.ParentAccount)
            .OrderBy(a => a.AccountType)
            .ThenBy(a => a.AccountCode)
            .ToListAsync(cancellationToken);

        // Convert to DTOs
        var accountDtos = allAccounts.Select(a => new AccountDto
        {
            Id = a.Id,
            AccountCode = a.AccountCode,
            AccountName = a.AccountName,
            AccountType = a.AccountType,
            Balance = a.Balance,
            IsActive = a.IsActive,
            Description = a.Description,
            ParentAccountId = a.ParentAccountId,
            ParentAccountName = a.ParentAccount?.AccountName
        }).ToList();

        // Build hierarchy and calculate levels
        var result = BuildHierarchy(accountDtos);
        
        // Flatten the hierarchy for tabular display with proper indentation levels
        return FlattenHierarchy(result);
    }

    private List<AccountDto> BuildHierarchy(List<AccountDto> accounts)
    {
        var accountLookup = accounts.ToDictionary(a => a.Id);
        var rootAccounts = new List<AccountDto>();

        foreach (var account in accounts)
        {
            if (account.ParentAccountId == null)
            {
                // This is a root account
                rootAccounts.Add(account);
            }
            else if (accountLookup.TryGetValue(account.ParentAccountId.Value, out var parent))
            {
                // Add to parent's sub-accounts
                parent.SubAccounts.Add(account);
            }
        }

        return rootAccounts;
    }

    private List<AccountDto> FlattenHierarchy(List<AccountDto> rootAccounts, int level = 0)
    {
        var result = new List<AccountDto>();

        foreach (var account in rootAccounts)
        {
            account.Level = level;
            result.Add(account);
            
            // Recursively add sub-accounts
            if (account.SubAccounts.Any())
            {
                var subAccountsFlattened = FlattenHierarchy(account.SubAccounts, level + 1);
                result.AddRange(subAccountsFlattened);
            }
        }

        return result;
    }
}