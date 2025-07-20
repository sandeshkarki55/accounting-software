using AccountingApi.DTOs.Dashboard;

namespace AccountingApi.Features.Dashboard
{
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync();

        Task<InvoiceStatusDistributionDto> GetInvoiceStatusDistributionAsync();

        Task<MonthlyRevenueTrendDto> GetMonthlyRevenueTrendAsync();

        Task<TopCustomersDto> GetTopCustomersAsync(int limit = 5);

        Task<RevenueVsExpensesDto> GetRevenueVsExpensesAsync();

        Task<PaymentTrendDto> GetPaymentTrendAsync(int months = 6);

        Task<AccountBalanceOverviewDto> GetAccountBalanceOverviewAsync();
    }
}