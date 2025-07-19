using MediatR;
using AccountingApi.Infrastructure;
using AccountingApi.DTOs.Dashboard;
using AccountingApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingApi.Features.Dashboard.Handlers
{
    public class GetRevenueVsExpensesQuery : IRequest<RevenueVsExpensesDto> { }

    public class GetRevenueVsExpensesHandler : IRequestHandler<GetRevenueVsExpensesQuery, RevenueVsExpensesDto>
    {
        private readonly AccountingDbContext _context;
        public GetRevenueVsExpensesHandler(AccountingDbContext context) => _context = context;

        public async Task<RevenueVsExpensesDto> Handle(GetRevenueVsExpensesQuery request, CancellationToken cancellationToken)
        {
            var currentYear = DateTime.Now.Year;
            var months = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            var revenueData = new decimal[12];
            var expenseData = new decimal[12];

            var monthlyRevenue = await _context.Invoices
                .AsNoTracking()
                .Where(i => i.Status == InvoiceStatus.Paid && i.InvoiceDate.Year == currentYear)
                .GroupBy(i => i.InvoiceDate.Month)
                .Select(g => new { Month = g.Key, Revenue = g.Sum(i => i.TotalAmount) })
                .ToListAsync(cancellationToken);

            foreach (var item in monthlyRevenue)
            {
                revenueData[item.Month - 1] = item.Revenue;
            }

            var expenseAccounts = await _context.Accounts
                .AsNoTracking()
                .Where(a => a.AccountType == AccountType.Expense && a.Balance > 0)
                .ToListAsync(cancellationToken);

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
    }
}
