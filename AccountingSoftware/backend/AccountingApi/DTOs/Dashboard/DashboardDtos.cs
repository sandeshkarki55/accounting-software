namespace AccountingApi.DTOs.Dashboard
{
    /// <summary>
    /// Dashboard statistics/KPI data
    /// </summary>
    public class DashboardStatsDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal OutstandingInvoices { get; set; }
        public int ActiveCustomers { get; set; }
        public decimal AverageInvoiceValue { get; set; }
        public decimal OverdueAmount { get; set; }
        public int TotalInvoiceCount { get; set; }
        public int PaidInvoicesCount { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal PreviousMonthRevenue { get; set; }
        public decimal RevenueChange { get; set; }
        public decimal PaymentRate { get; set; }
    }

    /// <summary>
    /// Invoice status distribution for pie chart
    /// </summary>
    public class InvoiceStatusDistributionDto
    {
        public List<string> Labels { get; set; } = new();
        public List<int> Data { get; set; } = new();
        public List<string> BackgroundColors { get; set; } = new();
    }

    /// <summary>
    /// Monthly revenue trend for line chart
    /// </summary>
    public class MonthlyRevenueTrendDto
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> Data { get; set; } = new();
    }

    /// <summary>
    /// Top customers by revenue for doughnut chart
    /// </summary>
    public class TopCustomersDto
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> Data { get; set; } = new();
        public List<string> BackgroundColors { get; set; } = new();
    }

    /// <summary>
    /// Revenue vs expenses for bar chart
    /// </summary>
    public class RevenueVsExpensesDto
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> RevenueData { get; set; } = new();
        public List<decimal> ExpensesData { get; set; } = new();
    }

    /// <summary>
    /// Payment trend for line chart
    /// </summary>
    public class PaymentTrendDto
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> Data { get; set; } = new();
    }

    /// <summary>
    /// Account balance overview for pie chart
    /// </summary>
    public class AccountBalanceOverviewDto
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> Data { get; set; } = new();
        public List<string> BackgroundColors { get; set; } = new();
    }
}