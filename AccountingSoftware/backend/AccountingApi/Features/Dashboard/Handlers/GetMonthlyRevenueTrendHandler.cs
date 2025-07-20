using AccountingApi.DTOs.Dashboard;
using AccountingApi.Infrastructure;
using AccountingApi.Models;

using Microsoft.EntityFrameworkCore;

using MyMediator;

namespace AccountingApi.Features.Dashboard.Handlers
{
    public class GetMonthlyRevenueTrendQuery : IRequest<MonthlyRevenueTrendDto>
    { }

    public class GetMonthlyRevenueTrendHandler : IRequestHandler<GetMonthlyRevenueTrendQuery, MonthlyRevenueTrendDto>
    {
        private readonly AccountingDbContext _context;

        public GetMonthlyRevenueTrendHandler(AccountingDbContext context) => _context = context;

        public async Task<MonthlyRevenueTrendDto> Handle(GetMonthlyRevenueTrendQuery request, CancellationToken cancellationToken)
        {
            var currentYear = DateTime.Now.Year;
            var monthlyData = new decimal[12];
            var labels = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

            var monthlyRevenue = await _context.Invoices
                .AsNoTracking()
                .Where(i => i.Status == InvoiceStatus.Paid && i.InvoiceDate.Year == currentYear)
                .GroupBy(i => i.InvoiceDate.Month)
                .Select(g => new { Month = g.Key, Revenue = g.Sum(i => i.TotalAmount) })
                .ToListAsync(cancellationToken);

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
    }
}