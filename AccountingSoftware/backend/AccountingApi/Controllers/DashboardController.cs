using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AccountingApi.Features.Dashboard;
using AccountingApi.DTOs.Dashboard;

namespace AccountingApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : BaseController
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Get dashboard statistics (KPI data)
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
        {
            try
            {
                var stats = await _dashboardService.GetDashboardStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to retrieve dashboard statistics", error = ex.Message });
            }
        }

        /// <summary>
        /// Get invoice status distribution data for chart
        /// </summary>
        [HttpGet("invoice-status-distribution")]
        public async Task<ActionResult<InvoiceStatusDistributionDto>> GetInvoiceStatusDistribution()
        {
            try
            {
                var data = await _dashboardService.GetInvoiceStatusDistributionAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to retrieve invoice status distribution", error = ex.Message });
            }
        }

        /// <summary>
        /// Get monthly revenue trend data for chart
        /// </summary>
        [HttpGet("monthly-revenue-trend")]
        public async Task<ActionResult<MonthlyRevenueTrendDto>> GetMonthlyRevenueTrend()
        {
            try
            {
                var data = await _dashboardService.GetMonthlyRevenueTrendAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to retrieve monthly revenue trend", error = ex.Message });
            }
        }

        /// <summary>
        /// Get top customers by revenue data for chart
        /// </summary>
        [HttpGet("top-customers")]
        public async Task<ActionResult<TopCustomersDto>> GetTopCustomers([FromQuery] int limit = 5)
        {
            try
            {
                var data = await _dashboardService.GetTopCustomersAsync(limit);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to retrieve top customers data", error = ex.Message });
            }
        }

        /// <summary>
        /// Get revenue vs expenses data for chart
        /// </summary>
        [HttpGet("revenue-vs-expenses")]
        public async Task<ActionResult<RevenueVsExpensesDto>> GetRevenueVsExpenses()
        {
            try
            {
                var data = await _dashboardService.GetRevenueVsExpensesAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to retrieve revenue vs expenses data", error = ex.Message });
            }
        }

        /// <summary>
        /// Get payment trend data for chart
        /// </summary>
        [HttpGet("payment-trend")]
        public async Task<ActionResult<PaymentTrendDto>> GetPaymentTrend([FromQuery] int months = 6)
        {
            try
            {
                var data = await _dashboardService.GetPaymentTrendAsync(months);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to retrieve payment trend data", error = ex.Message });
            }
        }

        /// <summary>
        /// Get account balance overview data for chart
        /// </summary>
        [HttpGet("account-balance-overview")]
        public async Task<ActionResult<AccountBalanceOverviewDto>> GetAccountBalanceOverview()
        {
            try
            {
                var data = await _dashboardService.GetAccountBalanceOverviewAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to retrieve account balance overview", error = ex.Message });
            }
        }
    }
}
