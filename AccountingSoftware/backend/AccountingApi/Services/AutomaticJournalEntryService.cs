using Microsoft.EntityFrameworkCore;
using AccountingApi.Infrastructure;
using AccountingApi.Models;

namespace AccountingApi.Services;

/// <summary>
/// Service for automatically creating journal entries from business transactions
/// </summary>
public class AutomaticJournalEntryService(
    AccountingDbContext context,
    IAccountConfigurationService accountConfig) : IAutomaticJournalEntryService
{
    /// <summary>
    /// Creates journal entry when an invoice is created
    /// DR: Accounts Receivable
    /// CR: Revenue (and Sales Tax Payable if applicable)
    /// </summary>
    public async Task<JournalEntry> CreateInvoiceJournalEntryAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        // Get the required accounts using configuration
        var accountsReceivableAccount = await GetAccountByCodeAsync(
            accountConfig.GetAccountsReceivableCode(), 
            "Accounts Receivable", 
            AccountType.Asset, 
            cancellationToken);
        
        var revenueAccount = await GetAccountByCodeAsync(
            accountConfig.GetRevenueAccountCode(), 
            "Revenue", 
            AccountType.Revenue, 
            cancellationToken);
        
        var salesTaxPayableAccount = invoice.TaxAmount > 0 
            ? await GetAccountByCodeAsync(
                accountConfig.GetSalesTaxPayableCode(), 
                "Sales Tax Payable", 
                AccountType.Liability, 
                cancellationToken)
            : null;

        // Generate journal entry number
        var entryNumber = await GenerateJournalEntryNumberAsync();

        var journalEntry = new JournalEntry
        {
            EntryNumber = entryNumber,
            TransactionDate = invoice.InvoiceDate,
            Description = $"Invoice {invoice.InvoiceNumber} - {invoice.Customer?.CompanyName ?? "Customer"}",
            Reference = invoice.InvoiceNumber,
            IsPosted = true, // Auto-post automatic entries
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            UpdatedBy = "System"
        };

        // Debit: Accounts Receivable (full invoice amount)
        journalEntry.Lines.Add(new JournalEntryLine
        {
            AccountId = accountsReceivableAccount.Id,
            DebitAmount = invoice.TotalAmount,
            CreditAmount = 0,
            Description = $"Invoice {invoice.InvoiceNumber}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            UpdatedBy = "System"
        });

        // Credit: Revenue (subtotal + discount)
        var revenueAmount = invoice.SubTotal - invoice.DiscountAmount;
        if (revenueAmount > 0)
        {
            journalEntry.Lines.Add(new JournalEntryLine
            {
                AccountId = revenueAccount.Id,
                DebitAmount = 0,
                CreditAmount = revenueAmount,
                Description = $"Revenue from Invoice {invoice.InvoiceNumber}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            });
        }

        // Credit: Sales Tax Payable (if applicable)
        if (invoice.TaxAmount > 0 && salesTaxPayableAccount != null)
        {
            journalEntry.Lines.Add(new JournalEntryLine
            {
                AccountId = salesTaxPayableAccount.Id,
                DebitAmount = 0,
                CreditAmount = invoice.TaxAmount,
                Description = $"Sales Tax from Invoice {invoice.InvoiceNumber}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            });
        }

        // Calculate total amount
        journalEntry.TotalAmount = journalEntry.Lines.Sum(l => Math.Max(l.DebitAmount, l.CreditAmount));

        context.JournalEntries.Add(journalEntry);
        await context.SaveChangesAsync(cancellationToken);

        // Update account balances
        await UpdateAccountBalancesAsync(journalEntry, cancellationToken);

        return journalEntry;
    }

    /// <summary>
    /// Creates journal entry when an invoice is paid
    /// DR: Cash/Bank Account
    /// CR: Accounts Receivable
    /// </summary>
    public async Task<JournalEntry> CreatePaymentJournalEntryAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        // Get the required accounts using configuration
        var cashAccount = await GetAccountByCodeAsync(
            accountConfig.GetCashAccountCode(), 
            "Cash", 
            AccountType.Asset, 
            cancellationToken);
        
        var accountsReceivableAccount = await GetAccountByCodeAsync(
            accountConfig.GetAccountsReceivableCode(), 
            "Accounts Receivable", 
            AccountType.Asset, 
            cancellationToken);

        // Generate journal entry number
        var entryNumber = await GenerateJournalEntryNumberAsync();

        var journalEntry = new JournalEntry
        {
            EntryNumber = entryNumber,
            TransactionDate = invoice.PaidDate ?? DateTime.UtcNow,
            Description = $"Payment received for Invoice {invoice.InvoiceNumber} - {invoice.Customer?.CompanyName ?? "Customer"}",
            Reference = $"{invoice.InvoiceNumber}-PAY",
            IsPosted = true, // Auto-post automatic entries
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            UpdatedBy = "System"
        };

        // Debit: Cash (payment amount)
        journalEntry.Lines.Add(new JournalEntryLine
        {
            AccountId = cashAccount.Id,
            DebitAmount = invoice.TotalAmount,
            CreditAmount = 0,
            Description = $"Payment for Invoice {invoice.InvoiceNumber}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            UpdatedBy = "System"
        });

        // Credit: Accounts Receivable (payment amount)
        journalEntry.Lines.Add(new JournalEntryLine
        {
            AccountId = accountsReceivableAccount.Id,
            DebitAmount = 0,
            CreditAmount = invoice.TotalAmount,
            Description = $"Payment for Invoice {invoice.InvoiceNumber}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            UpdatedBy = "System"
        });

        // Calculate total amount
        journalEntry.TotalAmount = journalEntry.Lines.Sum(l => Math.Max(l.DebitAmount, l.CreditAmount));

        context.JournalEntries.Add(journalEntry);
        await context.SaveChangesAsync(cancellationToken);

        // Update account balances
        await UpdateAccountBalancesAsync(journalEntry, cancellationToken);

        return journalEntry;
    }

    /// <summary>
    /// Creates reversal journal entry when an invoice is cancelled
    /// </summary>
    public async Task<JournalEntry?> CreateCancellationJournalEntryAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        // Find the original invoice journal entry
        var originalEntry = await context.JournalEntries
            .Include(je => je.Lines)
            .ThenInclude(l => l.Account)
            .FirstOrDefaultAsync(je => je.Reference == invoice.InvoiceNumber && !je.IsDeleted, cancellationToken);

        if (originalEntry == null)
            return null; // No original entry found

        // Generate journal entry number
        var entryNumber = await GenerateJournalEntryNumberAsync();

        var reversalEntry = new JournalEntry
        {
            EntryNumber = entryNumber,
            TransactionDate = DateTime.UtcNow,
            Description = $"Cancellation of Invoice {invoice.InvoiceNumber} - {invoice.Customer?.CompanyName ?? "Customer"}",
            Reference = $"{invoice.InvoiceNumber}-CANCEL",
            IsPosted = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            UpdatedBy = "System"
        };

        // Create reverse entries (flip debits and credits)
        foreach (var originalLine in originalEntry.Lines.Where(l => !l.IsDeleted))
        {
            reversalEntry.Lines.Add(new JournalEntryLine
            {
                AccountId = originalLine.AccountId,
                DebitAmount = originalLine.CreditAmount, // Flip
                CreditAmount = originalLine.DebitAmount, // Flip
                Description = $"Reversal: {originalLine.Description}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            });
        }

        // Calculate total amount
        reversalEntry.TotalAmount = reversalEntry.Lines.Sum(l => Math.Max(l.DebitAmount, l.CreditAmount));

        context.JournalEntries.Add(reversalEntry);
        await context.SaveChangesAsync(cancellationToken);

        // Update account balances
        await UpdateAccountBalancesAsync(reversalEntry, cancellationToken);

        return reversalEntry;
    }

    #region Private Helper Methods

    /// <summary>
    /// Gets or creates an account by code
    /// </summary>
    private async Task<Account> GetAccountByCodeAsync(string accountCode, string accountName, AccountType accountType, CancellationToken cancellationToken)
    {
        var account = await context.Accounts
            .FirstOrDefaultAsync(a => a.AccountCode == accountCode && !a.IsDeleted, cancellationToken);

        if (account == null)
        {
            // Create the account if it doesn't exist
            account = new Account
            {
                AccountCode = accountCode,
                AccountName = accountName,
                AccountType = accountType,
                Balance = 0,
                IsActive = true,
                Description = $"Auto-created for automatic journal entries",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            };

            context.Accounts.Add(account);
            await context.SaveChangesAsync(cancellationToken);
        }

        return account;
    }

    /// <summary>
    /// Generates a unique journal entry number
    /// </summary>
    private async Task<string> GenerateJournalEntryNumberAsync()
    {
        var today = DateTime.UtcNow;
        var prefix = $"JE{today:yyyyMM}";
        
        var lastEntry = await context.JournalEntries
            .Where(je => je.EntryNumber.StartsWith(prefix))
            .OrderByDescending(je => je.EntryNumber)
            .FirstOrDefaultAsync();

        if (lastEntry == null)
        {
            return $"{prefix}0001";
        }

        // Extract the sequence number
        var lastNumber = lastEntry.EntryNumber.Substring(prefix.Length);
        if (int.TryParse(lastNumber, out var sequence))
        {
            return $"{prefix}{(sequence + 1):D4}";
        }

        return $"{prefix}0001";
    }

    /// <summary>
    /// Updates account balances based on journal entry lines
    /// </summary>
    private async Task UpdateAccountBalancesAsync(JournalEntry journalEntry, CancellationToken cancellationToken)
    {
        var accountIds = journalEntry.Lines.Select(l => l.AccountId).Distinct();
        var accounts = await context.Accounts
            .Where(a => accountIds.Contains(a.Id))
            .ToListAsync(cancellationToken);

        foreach (var account in accounts)
        {
            var lines = journalEntry.Lines.Where(l => l.AccountId == account.Id);
            var debitTotal = lines.Sum(l => l.DebitAmount);
            var creditTotal = lines.Sum(l => l.CreditAmount);

            // Update balance based on account type
            switch (account.AccountType)
            {
                case AccountType.Asset:
                case AccountType.Expense:
                    // Assets and Expenses increase with debits
                    account.Balance += (debitTotal - creditTotal);
                    break;

                case AccountType.Liability:
                case AccountType.Equity:
                case AccountType.Revenue:
                    // Liabilities, Equity, and Revenue increase with credits
                    account.Balance += (creditTotal - debitTotal);
                    break;
            }

            account.UpdatedAt = DateTime.UtcNow;
            account.UpdatedBy = "System";
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    #endregion
}
