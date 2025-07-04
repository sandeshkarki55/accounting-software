using Microsoft.EntityFrameworkCore;
using AccountingApi.Infrastructure;
using AccountingApi.Models;

namespace AccountingApi.Infrastructure.Seeds;

/// <summary>
/// Seeds basic chart of accounts for automatic journal entries
/// </summary>
public static class AccountSeeder
{
    /// <summary>
    /// Seeds the basic chart of accounts required for automatic journal entries
    /// </summary>
    public static async Task SeedAccountsAsync(AccountingDbContext context)
    {
        var defaultAccounts = GetDefaultChartOfAccounts();

        foreach (var accountData in defaultAccounts)
        {
            // Check if account already exists
            var existingAccount = await context.Accounts
                .FirstOrDefaultAsync(a => a.AccountCode == accountData.AccountCode && !a.IsDeleted);

            if (existingAccount == null)
            {
                var account = new Account
                {
                    AccountCode = accountData.AccountCode,
                    AccountName = accountData.AccountName,
                    AccountType = accountData.AccountType,
                    Balance = 0,
                    IsActive = true,
                    Description = accountData.Description,
                    ParentAccountId = accountData.ParentAccountId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    UpdatedBy = "System"
                };

                context.Accounts.Add(account);
            }
        }

        await context.SaveChangesAsync();

        // Update parent account IDs for accounts that need them
        await UpdateParentAccountIds(context);
    }

    /// <summary>
    /// Defines the default chart of accounts structure
    /// </summary>
    private static List<DefaultAccount> GetDefaultChartOfAccounts()
    {
        return new List<DefaultAccount>
        {
            // ASSETS (1000-1999)
            new("1000", "Cash and Cash Equivalents", AccountType.Asset, "Primary cash account for payments and receipts"),
            new("1050", "Petty Cash", AccountType.Asset, "Small cash fund for minor expenses"),
            new("1100", "Checking Account", AccountType.Asset, "Primary business checking account"),
            new("1150", "Savings Account", AccountType.Asset, "Business savings account"),
            new("1200", "Accounts Receivable", AccountType.Asset, "Money owed by customers for goods/services sold on credit"),
            new("1250", "Allowance for Doubtful Accounts", AccountType.Asset, "Estimated uncollectible receivables"),
            new("1300", "Inventory", AccountType.Asset, "Cost of goods held for sale"),
            new("1400", "Prepaid Expenses", AccountType.Asset, "Expenses paid in advance"),
            new("1500", "Office Equipment", AccountType.Asset, "Computers, furniture, and office equipment"),
            new("1600", "Accumulated Depreciation - Office Equipment", AccountType.Asset, "Accumulated depreciation on office equipment"),
            new("1700", "Vehicles", AccountType.Asset, "Company vehicles"),
            new("1800", "Accumulated Depreciation - Vehicles", AccountType.Asset, "Accumulated depreciation on vehicles"),

            // LIABILITIES (2000-2999)
            new("2000", "Accounts Payable", AccountType.Liability, "Money owed to suppliers and vendors"),
            new("2100", "Accrued Expenses", AccountType.Liability, "Expenses incurred but not yet paid"),
            new("2200", "Short-term Loans", AccountType.Liability, "Loans payable within one year"),
            new("2300", "Sales Tax Payable", AccountType.Liability, "Sales tax collected from customers"),
            new("2400", "Payroll Tax Payable", AccountType.Liability, "Employee and employer payroll taxes"),
            new("2500", "Income Tax Payable", AccountType.Liability, "Income taxes owed"),
            new("2600", "Long-term Debt", AccountType.Liability, "Loans and debt payable over one year"),
            new("2700", "Deferred Revenue", AccountType.Liability, "Payments received for services not yet provided"),

            // EQUITY (3000-3999)
            new("3000", "Owner's Equity", AccountType.Equity, "Owner's investment in the business"),
            new("3100", "Retained Earnings", AccountType.Equity, "Accumulated profits retained in the business"),
            new("3200", "Owner's Draw", AccountType.Equity, "Money withdrawn by owner for personal use"),

            // REVENUE (4000-4999)
            new("4000", "Revenue", AccountType.Revenue, "Primary revenue from sales of goods/services"),
            new("4100", "Service Revenue", AccountType.Revenue, "Revenue from providing services"),
            new("4200", "Product Sales", AccountType.Revenue, "Revenue from selling products"),
            new("4300", "Interest Income", AccountType.Revenue, "Interest earned on investments and deposits"),
            new("4400", "Other Income", AccountType.Revenue, "Miscellaneous income"),

            // EXPENSES (5000-5999)
            new("5000", "Cost of Goods Sold", AccountType.Expense, "Direct costs of producing goods sold"),
            new("5100", "Salaries and Wages", AccountType.Expense, "Employee compensation"),
            new("5200", "Rent Expense", AccountType.Expense, "Office and facility rent"),
            new("5300", "Utilities Expense", AccountType.Expense, "Electricity, water, gas, internet"),
            new("5400", "Office Supplies", AccountType.Expense, "Supplies used in daily operations"),
            new("5500", "Insurance Expense", AccountType.Expense, "Business insurance premiums"),
            new("5600", "Professional Fees", AccountType.Expense, "Legal, accounting, consulting fees"),
            new("5700", "Marketing and Advertising", AccountType.Expense, "Promotional and advertising costs"),
            new("5800", "Travel Expense", AccountType.Expense, "Business travel costs"),
            new("5900", "Depreciation Expense", AccountType.Expense, "Depreciation of fixed assets"),
            new("5950", "Bank Fees", AccountType.Expense, "Banking and transaction fees"),
            new("5990", "Miscellaneous Expense", AccountType.Expense, "Other business expenses")
        };
    }

    /// <summary>
    /// Updates parent account relationships after initial seeding
    /// </summary>
    private static async Task UpdateParentAccountIds(AccountingDbContext context)
    {
        // Define parent-child relationships
        var parentChildRelationships = new Dictionary<string, string>
        {
            // Accumulated Depreciation accounts are contra-assets under their respective asset accounts
            { "1600", "1500" }, // Accumulated Depreciation - Office Equipment under Office Equipment
            { "1800", "1700" }, // Accumulated Depreciation - Vehicles under Vehicles
        };

        foreach (var relationship in parentChildRelationships)
        {
            var childCode = relationship.Key;
            var parentCode = relationship.Value;

            var childAccount = await context.Accounts
                .FirstOrDefaultAsync(a => a.AccountCode == childCode && !a.IsDeleted);
            var parentAccount = await context.Accounts
                .FirstOrDefaultAsync(a => a.AccountCode == parentCode && !a.IsDeleted);

            if (childAccount != null && parentAccount != null)
            {
                childAccount.ParentAccountId = parentAccount.Id;
                childAccount.UpdatedAt = DateTime.UtcNow;
                childAccount.UpdatedBy = "System";
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Helper record for defining default accounts
    /// </summary>
    private record DefaultAccount(
        string AccountCode,
        string AccountName,
        AccountType AccountType,
        string Description,
        int? ParentAccountId = null);
}
