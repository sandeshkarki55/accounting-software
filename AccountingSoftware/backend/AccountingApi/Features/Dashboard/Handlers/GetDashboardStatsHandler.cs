using MediatR;
using AccountingApi.Infrastructure;
using AccountingApi.DTOs.Dashboard;
using AccountingApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingApi.Features.Dashboard.Handlers
{
    public class GetDashboardStatsQuery : IRequest<DashboardStatsDto> { }

    public class GetDashboardStatsHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
    {
        private readonly AccountingDbContext _context;
        public GetDashboardStatsHandler(AccountingDbContext context) => _context = context;

        public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var previousMonth = currentMonth == 1 ? 12 : currentMonth - 1;
            var previousMonthYear = currentMonth == 1 ? currentYear - 1 : currentYear;

            var invoices = await _context.Invoices.AsNoTracking().ToListAsync(cancellationToken);
            var customers = await _context.Customers.AsNoTracking().ToListAsync(cancellationToken);

            var totalRevenue = invoices.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.TotalAmount);
            var outstandingInvoices = invoices.Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled).Sum(i => i.TotalAmount);
            var overdueAmount = invoices.Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled && i.DueDate < DateTime.Now).Sum(i => i.TotalAmount);
            var activeCustomers = customers.Count(c => c.IsActive);
            var averageInvoiceValue = invoices.Any() ? invoices.Average(i => i.TotalAmount) : 0;
            var totalInvoiceCount = invoices.Count;
            var paidInvoicesCount = invoices.Count(i => i.Status == InvoiceStatus.Paid);
            var monthlyRevenue = invoices.Where(i => i.Status == InvoiceStatus.Paid && i.InvoiceDate.Month == currentMonth && i.InvoiceDate.Year == currentYear).Sum(i => i.TotalAmount);
            var previousMonthRevenue = invoices.Where(i => i.Status == InvoiceStatus.Paid && i.InvoiceDate.Month == previousMonth && i.InvoiceDate.Year == previousMonthYear).Sum(i => i.TotalAmount);
            var revenueChange = previousMonthRevenue == 0 && monthlyRevenue > 0 ? 100 : previousMonthRevenue == 0 ? 0 : ((monthlyRevenue - previousMonthRevenue) / previousMonthRevenue) * 100;
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
    }
}
