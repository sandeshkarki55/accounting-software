using AccountingApi.Models;

namespace AccountingApi.DTOs;

// Account DTOs
public class AccountDto
{
    public int Id { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public decimal Balance { get; set; }
    public bool IsActive { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? ParentAccountId { get; set; }
    public string? ParentAccountName { get; set; }
    public int Level { get; set; } = 0; // For tree structure display
    public List<AccountDto> SubAccounts { get; set; } = []; // For hierarchical display
}

public class CreateAccountDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? ParentAccountId { get; set; }
}

public class UpdateAccountDto
{
    public string AccountName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}