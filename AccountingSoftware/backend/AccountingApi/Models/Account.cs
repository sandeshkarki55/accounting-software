namespace AccountingApi.Models;

// Account entity for chart of accounts
public class Account : BaseEntity
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public decimal Balance { get; set; }
    public bool IsActive { get; set; } = true;
    public string Description { get; set; } = string.Empty;
    
    // Self-referencing relationship for account hierarchy
    public int? ParentAccountId { get; set; }
    public Account? ParentAccount { get; set; }
    public ICollection<Account> SubAccounts { get; set; } = [];
    
    // Navigation property for journal entry lines (not direct journal entries)
    // This properly represents that accounts are affected by individual transaction lines
    public ICollection<JournalEntryLine> JournalEntryLines { get; set; } = [];
}

// Account types enum
public enum AccountType
{
    Asset,
    Liability,
    Equity,
    Revenue,
    Expense
}