using Microsoft.EntityFrameworkCore;
using AccountingApi.Infrastructure;
using AccountingApi.Models;
using AccountingApi.DTOs.Dashboard;
using AccountingApi.Features.Dashboard;

namespace AccountingApi.Features.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly AccountingDbContext _context;

        public DashboardService(AccountingDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var previousMonth = currentMonth == 1 ? 12 : currentMonth - 1;
            var previousMonthYear = currentMonth == 1 ? currentYear - 1 : currentYear;

            var invoices = await _context.Invoices.AsNoTracking().ToListAsync();
            var customers = await _context.Customers.AsNoTracking().ToListAsync();

            // Calculate total revenue (from paid invoices)
            var totalRevenue = invoices
                .Where(i => i.Status == InvoiceStatus.Paid)
                .Sum(i => i.TotalAmount);

            // Calculate outstanding invoices
            var outstandingInvoices = invoices
                .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled)
                .Sum(i => i.TotalAmount);

            // Calculate overdue amount
            var overdueAmount = invoices
                .Where(i => i.Status != InvoiceStatus.Paid && 
                           i.Status != InvoiceStatus.Cancelled && 
                           i.DueDate < DateTime.Now)
                .Sum(i => i.TotalAmount);

            // Active customers
            var activeCustomers = customers.Count(c => c.IsActive);

            // Average invoice value
            var averageInvoiceValue = invoices.Any() ? invoices.Average(i => i.TotalAmount) : 0;

            // Invoice counts
            var totalInvoiceCount = invoices.Count;
            var paidInvoicesCount = invoices.Count(i => i.Status == InvoiceStatus.Paid);

            // Monthly revenue calculations
            var monthlyRevenue = invoices
                .Where(i => i.Status == InvoiceStatus.Paid &&
                           i.InvoiceDate.Month == currentMonth &&
                           i.InvoiceDate.Year == currentYear)
                .Sum(i => i.TotalAmount);

            var previousMonthRevenue = invoices
                .Where(i => i.Status == InvoiceStatus.Paid &&
                           i.InvoiceDate.Month == previousMonth &&
                           i.InvoiceDate.Year == previousMonthYear)
                .Sum(i => i.TotalAmount);

            // Calculate revenue change percentage
            var revenueChange = previousMonthRevenue == 0 && monthlyRevenue > 0 ? 100 :
                               previousMonthRevenue == 0 ? 0 :
                               ((monthlyRevenue - previousMonthRevenue) / previousMonthRevenue) * 100;

            // Payment rate
            var paymentRate = totalInvoiceCount > 0 ? (decimal)paidInvoicesCount / totalInvoiceCount * 100 : 0;

            return new DashboardStatsDto
            {
                TotalRevenue = totalRevenue,
                OutstandingInvoices = outstandingInvoices,
                ActiveCustomers = activeCustomers,
                AverageInvoiceValue = averageInvoiceValue,
                OverdueAmount = overdueAmount,
                TotalInvoiceCount = totalInvoiceCount,
                PaidInvoicesCount = paidInvoicesCount,
                MonthlyRevenue = monthlyRevenue,
                PreviousMonthRevenue = previousMonthRevenue,
                RevenueChange = revenueChange,
                PaymentRate = paymentRate
            };
        }

        public async Task<InvoiceStatusDistributionDto> GetInvoiceStatusDistributionAsync()
        {
            var statusCounts = await _context.Invoices
                .AsNoTracking()
                .GroupBy(i => i.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var labels = new List<string>();
            var data = new List<int>();
            var colors = new List<string> { "#6c757d", "#0d6efd", "#198754", "#dc3545", "#343a40", "#ffc107" };

            // Ensure all statuses are represented
            var allStatuses = Enum.GetValues<InvoiceStatus>();
            var statusLabels = new Dictionary<InvoiceStatus, string>
            {
                { InvoiceStatus.Draft, "Draft" },
                { InvoiceStatus.Sent, "Sent" },
                { InvoiceStatus.Paid, "Paid" },
                { InvoiceStatus.Overdue, "Overdue" },
                { InvoiceStatus.Cancelled, "Cancelled" },
                { InvoiceStatus.PartiallyPaid, "Partially Paid" }
            };

            foreach (var status in allStatuses)
            {
                var count = statusCounts.FirstOrDefault(sc => sc.Status == status)?.Count ?? 0;
                labels.Add(statusLabels[status]);
                data.Add(count);
            }

            return new InvoiceStatusDistributionDto
            {
                Labels = labels,
                Data = data,
                BackgroundColors = colors
            };
        }

        public async Task<MonthlyRevenueTrendDto> GetMonthlyRevenueTrendAsync()
        {
            var currentYear = DateTime.Now.Year;
            var monthlyData = new decimal[12];
            var labels = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

            var monthlyRevenue = await _context.Invoices
                .AsNoTracking()
                .Where(i => i.Status == InvoiceStatus.Paid && i.InvoiceDate.Year == currentYear)
                .GroupBy(i => i.InvoiceDate.Month)
                .Select(g => new { Month = g.Key, Revenue = g.Sum(i => i.TotalAmount) })
                .ToListAsync();

            foreach (var item in monthlyRevenue)
            {
                monthlyData[item.Month - 1] = item.Revenue;
            }

            return new MonthlyRevenueTrendDto
            {
                Labels = labels,
                Data = monthlyData.ToList()
            };
        }

        public async Task<TopCustomersDto> GetTopCustomersAsync(int limit = 5)
        {
            var topCustomers = await _context.Invoices
                .AsNoTracking()
                .Include(i => i.Customer)
                .Where(i => i.Status == InvoiceStatus.Paid)
                .GroupBy(i => i.Customer.CompanyName)
                .Select(g => new { CustomerName = g.Key, Revenue = g.Sum(i => i.TotalAmount) })
                .OrderByDescending(x => x.Revenue)
                .Take(limit)
                .ToListAsync();

            var colors = new List<string> { "#198754", "#0d6efd", "#ffc107", "#fd7e14", "#6f42c1" };

            return new TopCustomersDto
            {
                Labels = topCustomers.Select(c => c.CustomerName).ToList(),
                Data = topCustomers.Select(c => c.Revenue).ToList(),
                BackgroundColors = colors.Take(topCustomers.Count).ToList()
            };
        }

        public async Task<RevenueVsExpensesDto> GetRevenueVsExpensesAsync()
        {
            var currentYear = DateTime.Now.Year;
            var months = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            var revenueData = new decimal[12];
            var expenseData = new decimal[12];

            // Calculate revenue from paid invoices
            var monthlyRevenue = await _context.Invoices
                .AsNoTracking()
                .Where(i => i.Status == InvoiceStatus.Paid && i.InvoiceDate.Year == currentYear)
                .GroupBy(i => i.InvoiceDate.Month)
                .Select(g => new { Month = g.Key, Revenue = g.Sum(i => i.TotalAmount) })
                .ToListAsync();

            foreach (var item in monthlyRevenue)
            {
                revenueData[item.Month - 1] = item.Revenue;
            }

            // Calculate expenses from expense accounts (AccountType.Expense = 4)
            var expenseAccounts = await _context.Accounts
                .AsNoTracking()
                .Where(a => a.AccountType == AccountType.Expense && a.Balance > 0)
                .ToListAsync();

            // For demo purposes, distribute the expense balance across the year
            // In a real system, you'd have transaction dates to properly allocate
            foreach (var account in expenseAccounts)
            {
                var monthlyExpense = account.Balance / 12;
                for (int i = 0; i < 12; i++)
                {
                    expenseData[i] += monthlyExpense;
                }
            }

            return new RevenueVsExpensesDto
            {
                Labels = months,
                RevenueData = revenueData.ToList(),
                ExpensesData = expenseData.ToList()
            };
        }

        public async Task<PaymentTrendDto> GetPaymentTrendAsync(int months = 6)
        {
            var labels = new List<string>();
            var data = new List<decimal>();
            var currentDate = DateTime.Now;

            for (int i = months - 1; i >= 0; i--)
            {
                var date = currentDate.AddMonths(-i);
                labels.Add(date.ToString("MMM yy"));

                var monthPayments = await _context.Invoices
                    .AsNoTracking()
                    .Where(i => i.Status == InvoiceStatus.Paid &&
                               i.InvoiceDate.Month == date.Month &&
                               i.InvoiceDate.Year == date.Year)
                    .SumAsync(i => i.TotalAmount);

                data.Add(monthPayments);
            }

            return new PaymentTrendDto
            {
                Labels = labels,
                Data = data
            };
        }

        public async Task<AccountBalanceOverviewDto> GetAccountBalanceOverviewAsync()
        {
            var accounts = await _context.Accounts
                .AsNoTracking()
                .Where(a => a.IsActive)
                .ToListAsync();

            var totalAssets = accounts.Where(a => a.AccountType == AccountType.Asset).Sum(a => a.Balance);
            var totalLiabilities = accounts.Where(a => a.AccountType == AccountType.Liability).Sum(a => a.Balance);
            var equity = totalAssets - totalLiabilities;

            var labels = new List<string> { "Assets", "Liabilities", "Equity" };
            var data = new List<decimal> { totalAssets, totalLiabilities, equity };
            var colors = new List<string> { "#28a745", "#dc3545", "#007bff" };

            return new AccountBalanceOverviewDto
            {
                Labels = labels,
                Data = data,
                BackgroundColors = colors
            };
        }
    }
}
