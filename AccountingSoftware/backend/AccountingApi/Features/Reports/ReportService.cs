using AccountingApi.DTOs.Reports;
using AccountingApi.Infrastructure;
using AccountingApi.Models;

using Microsoft.EntityFrameworkCore;

namespace AccountingApi.Features.Reports
{
    public class ReportService : IReportService
    {
        private readonly AccountingDbContext _context;

        public ReportService(AccountingDbContext context) => _context = context;

        public async Task<TrialBalanceDto> GetTrialBalanceAsync(DateTime? asOfDate)
        {
            var endDate = (asOfDate ?? DateTime.UtcNow).Date;

            var accounts = await _context.Accounts
                .AsNoTracking()
                .Where(a => a.IsActive)
                .OrderBy(a => a.AccountCode)
                .ToListAsync();

            // Get posted journal entries up to the asOfDate
            var journalLines = await _context.JournalEntryLines
                .AsNoTracking()
                .Include(jl => jl.JournalEntry)
                .Where(jl => jl.JournalEntry.IsPosted && jl.JournalEntry.TransactionDate.Date <= endDate)
                .ToListAsync();

            var lines = new List<TrialBalanceLineDto>();
            decimal totalDebits = 0, totalCredits = 0;

            foreach (var account in accounts)
            {
                var accountLines = journalLines.Where(jl => jl.AccountId == account.Id);
                var debits = accountLines.Sum(jl => jl.DebitAmount);
                var credits = accountLines.Sum(jl => jl.CreditAmount);

                // Normal balances: Assets/Expenses = Debit, Liabilities/Equity/Revenue = Credit
                decimal debitBalance, creditBalance;
                var netBalance = debits - credits;

                if (account.AccountType is AccountType.Asset or AccountType.Expense)
                {
                    debitBalance = netBalance > 0 ? netBalance : 0;
                    creditBalance = netBalance < 0 ? Math.Abs(netBalance) : 0;
                }
                else
                {
                    creditBalance = netBalance < 0 ? Math.Abs(netBalance) : 0;
                    debitBalance = netBalance > 0 ? netBalance : 0;
                }

                if (Math.Abs(debitBalance) > 0.01m || Math.Abs(creditBalance) > 0.01m)
                {
                    lines.Add(new TrialBalanceLineDto
                    {
                        AccountId = account.Id,
                        AccountCode = account.AccountCode,
                        AccountName = account.AccountName,
                        AccountType = account.AccountType.ToString(),
                        DebitBalance = debitBalance,
                        CreditBalance = creditBalance
                    });
                    totalDebits += debitBalance;
                    totalCredits += creditBalance;
                }
            }

            return new TrialBalanceDto { Lines = lines, TotalDebits = totalDebits, TotalCredits = totalCredits };
        }

        public async Task<IncomeStatementDto> GetIncomeStatementAsync(DateTime startDate, DateTime endDate)
        {
            var accounts = await _context.Accounts.AsNoTracking().Where(a => a.IsActive).ToListAsync();

            var journalLines = await _context.JournalEntryLines
                .AsNoTracking()
                .Include(jl => jl.JournalEntry)
                .Where(jl => jl.JournalEntry.IsPosted
                    && jl.JournalEntry.TransactionDate.Date >= startDate.Date
                    && jl.JournalEntry.TransactionDate.Date <= endDate.Date)
                .ToListAsync();

            var result = new IncomeStatementDto();

            foreach (var account in accounts.Where(a => a.AccountType == AccountType.Revenue))
            {
                var lines = journalLines.Where(jl => jl.AccountId == account.Id);
                var amount = lines.Sum(jl => jl.CreditAmount) - lines.Sum(jl => jl.DebitAmount);
                if (Math.Abs(amount) > 0.01m)
                    result.Revenue.Add(new IncomeStatementSectionDto { AccountId = account.Id, AccountCode = account.AccountCode, AccountName = account.AccountName, Amount = amount });
            }
            result.TotalRevenue = result.Revenue.Sum(r => r.Amount);

            // COGS: expense accounts with "COGS" or "cost of goods" in the name
            var cogsAccounts = accounts.Where(a => a.AccountType == AccountType.Expense
                && (a.AccountName.Contains("COGS", StringComparison.OrdinalIgnoreCase)
                    || a.AccountName.Contains("Cost of Goods", StringComparison.OrdinalIgnoreCase)));
            foreach (var account in cogsAccounts)
            {
                var amount = journalLines.Where(jl => jl.AccountId == account.Id).Sum(jl => jl.DebitAmount - jl.CreditAmount);
                if (Math.Abs(amount) > 0.01m)
                    result.CostOfGoodsSold.Add(new IncomeStatementSectionDto { AccountId = account.Id, AccountCode = account.AccountCode, AccountName = account.AccountName, Amount = amount });
            }
            result.TotalCOGS = result.CostOfGoodsSold.Sum(c => c.Amount);

            // Operating expenses: remaining expense accounts
            var operatingExpenseAccounts = accounts.Where(a => a.AccountType == AccountType.Expense && !cogsAccounts.Any(c => c.Id == a.Id));
            foreach (var account in operatingExpenseAccounts)
            {
                var amount = journalLines.Where(jl => jl.AccountId == account.Id).Sum(jl => jl.DebitAmount - jl.CreditAmount);
                if (Math.Abs(amount) > 0.01m)
                    result.OperatingExpenses.Add(new IncomeStatementSectionDto { AccountId = account.Id, AccountCode = account.AccountCode, AccountName = account.AccountName, Amount = amount });
            }
            result.TotalOperatingExpenses = result.OperatingExpenses.Sum(e => e.Amount);

            return result;
        }

        public async Task<BalanceSheetDto> GetBalanceSheetAsync(DateTime? asOfDate)
        {
            var endDate = (asOfDate ?? DateTime.UtcNow).Date;
            var accounts = await _context.Accounts.AsNoTracking().Where(a => a.IsActive).ToListAsync();
            var journalLines = await _context.JournalEntryLines
                .AsNoTracking()
                .Include(jl => jl.JournalEntry)
                .Where(jl => jl.JournalEntry.IsPosted && jl.JournalEntry.TransactionDate.Date <= endDate)
                .ToListAsync();

            var result = new BalanceSheetDto();

            foreach (var account in accounts.Where(a => a.AccountType == AccountType.Asset))
            {
                var netBalance = journalLines.Where(jl => jl.AccountId == account.Id).Sum(jl => jl.DebitAmount - jl.CreditAmount);
                result.Assets.Add(new BalanceSheetSectionDto { AccountId = account.Id, AccountCode = account.AccountCode, AccountName = account.AccountName, Balance = netBalance });
            }
            result.TotalAssets = result.Assets.Sum(a => a.Balance);

            foreach (var account in accounts.Where(a => a.AccountType == AccountType.Liability))
            {
                var netBalance = journalLines.Where(jl => jl.AccountId == account.Id).Sum(jl => jl.CreditAmount - jl.DebitAmount);
                result.Liabilities.Add(new BalanceSheetSectionDto { AccountId = account.Id, AccountCode = account.AccountCode, AccountName = account.AccountName, Balance = netBalance });
            }
            result.TotalLiabilities = result.Liabilities.Sum(l => l.Balance);

            foreach (var account in accounts.Where(a => a.AccountType == AccountType.Equity))
            {
                var netBalance = journalLines.Where(jl => jl.AccountId == account.Id).Sum(jl => jl.CreditAmount - jl.DebitAmount);
                result.Equity.Add(new BalanceSheetSectionDto { AccountId = account.Id, AccountCode = account.AccountCode, AccountName = account.AccountName, Balance = netBalance });
            }
            // Include retained earnings (Revenue - Expenses up to endDate)
            var revenueBalance = journalLines
                .Where(jl => accounts.FirstOrDefault(a => a.Id == jl.AccountId)?.AccountType == AccountType.Revenue)
                .Sum(jl => jl.CreditAmount - jl.DebitAmount);
            var expenseBalance = journalLines
                .Where(jl => accounts.FirstOrDefault(a => a.Id == jl.AccountId)?.AccountType == AccountType.Expense)
                .Sum(jl => jl.DebitAmount - jl.CreditAmount);
            var retainedEarnings = revenueBalance - expenseBalance;
            if (Math.Abs(retainedEarnings) > 0.01m)
            {
                result.Equity.Add(new BalanceSheetSectionDto { AccountId = 0, AccountCode = "RETAINED", AccountName = "Retained Earnings", Balance = retainedEarnings });
            }
            result.TotalEquity = result.Equity.Sum(e => e.Balance);

            return result;
        }

        public async Task<GeneralLedgerDto> GetGeneralLedgerAsync(int accountId, DateTime startDate, DateTime endDate)
        {
            var account = await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == accountId && a.IsActive)
                ?? throw new ArgumentException("Account not found");

            var journalLines = await _context.JournalEntryLines
                .AsNoTracking()
                .Include(jl => jl.JournalEntry)
                .Where(jl => jl.AccountId == accountId
                    && jl.JournalEntry.IsPosted
                    && jl.JournalEntry.TransactionDate.Date >= startDate.Date
                    && jl.JournalEntry.TransactionDate.Date <= endDate.Date)
                .OrderBy(jl => jl.JournalEntry.TransactionDate)
                .ThenBy(jl => jl.JournalEntry.EntryNumber)
                .ToListAsync();

            // Calculate opening balance
            var priorLines = await _context.JournalEntryLines
                .AsNoTracking()
                .Include(jl => jl.JournalEntry)
                .Where(jl => jl.AccountId == accountId
                    && jl.JournalEntry.IsPosted
                    && jl.JournalEntry.TransactionDate.Date < startDate.Date)
                .ToListAsync();

            var openingBalance = priorLines.Sum(jl => jl.DebitAmount - jl.CreditAmount);
            var runningBalance = openingBalance;

            var lines = new List<GeneralLedgerLineDto>();
            foreach (var line in journalLines)
            {
                runningBalance += line.DebitAmount - line.CreditAmount;
                lines.Add(new GeneralLedgerLineDto
                {
                    TransactionDate = line.JournalEntry.TransactionDate,
                    EntryNumber = line.JournalEntry.EntryNumber,
                    Description = line.Description,
                    DebitAmount = line.DebitAmount,
                    CreditAmount = line.CreditAmount,
                    RunningBalance = runningBalance
                });
            }

            return new GeneralLedgerDto
            {
                AccountId = account.Id,
                AccountCode = account.AccountCode,
                AccountName = account.AccountName,
                AccountType = account.AccountType.ToString(),
                Lines = lines,
                OpeningBalance = openingBalance,
                ClosingBalance = runningBalance
            };
        }

        public async Task<AgedReceivablesDto> GetAgedReceivablesAsync(DateTime? asOfDate)
        {
            var endDate = (asOfDate ?? DateTime.UtcNow).Date;

            var unpaidInvoices = await _context.Invoices
                .AsNoTracking()
                .Include(i => i.Customer)
                .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled)
                .ToListAsync();

            var customers = new List<AgedReceivablesCustomerDto>();
            var groupedByCustomer = unpaidInvoices.GroupBy(i => i.CustomerId);

            foreach (var group in groupedByCustomer)
            {
                var firstInvoice = group.First();
                var customer = new AgedReceivablesCustomerDto
                {
                    CustomerId = firstInvoice.CustomerId,
                    CustomerName = firstInvoice.Customer?.CompanyName ?? "Unknown",
                    CustomerCode = firstInvoice.Customer?.CustomerCode ?? ""
                };

                foreach (var invoice in group)
                {
                    var daysOverdue = (endDate - invoice.DueDate.Date).Days;
                    var invoiceDto = new AgedReceivableInvoiceDto
                    {
                        InvoiceId = invoice.Id,
                        InvoiceNumber = invoice.InvoiceNumber,
                        InvoiceDate = invoice.InvoiceDate,
                        DueDate = invoice.DueDate,
                        Amount = invoice.TotalAmount,
                        DaysOverdue = daysOverdue
                    };
                    customer.Invoices.Add(invoiceDto);

                    var amount = invoice.TotalAmount;
                    if (daysOverdue <= 0) customer.CurrentAmount += amount;
                    else if (daysOverdue <= 30) customer.Days1To30Amount += amount;
                    else if (daysOverdue <= 60) customer.Days31To60Amount += amount;
                    else if (daysOverdue <= 90) customer.Days61To90Amount += amount;
                    else customer.DaysOver90Amount += amount;
                }

                customers.Add(customer);
            }

            return new AgedReceivablesDto
            {
                Customers = customers.OrderByDescending(c => c.TotalOutstanding).ToList(),
                Summary = new AgedReceivablesSummaryDto
                {
                    TotalCurrent = customers.Sum(c => c.CurrentAmount),
                    TotalDays1To30 = customers.Sum(c => c.Days1To30Amount),
                    TotalDays31To60 = customers.Sum(c => c.Days31To60Amount),
                    TotalDays61To90 = customers.Sum(c => c.Days61To90Amount),
                    TotalDaysOver90 = customers.Sum(c => c.DaysOver90Amount)
                }
            };
        }
    }
}
