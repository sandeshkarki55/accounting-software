using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AccountingApi.DTOs.Dashboard;
using AccountingApi.Features.Dashboard.Handlers;
using MyMediator;

namespace AccountingApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : BaseController
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get dashboard statistics (KPI data)
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
        {
            var stats = await _mediator.Send(new GetDashboardStatsQuery());
            return Ok(stats);
        }

        /// <summary>
        /// Get invoice status distribution data for chart
        /// </summary>
        [HttpGet("invoice-status-distribution")]
        public async Task<ActionResult<InvoiceStatusDistributionDto>> GetInvoiceStatusDistribution()
        {
            var data = await _mediator.Send(new GetInvoiceStatusDistributionQuery());
            return Ok(data);
        }

        /// <summary>
        /// Get monthly revenue trend data for chart
        /// </summary>
        [HttpGet("monthly-revenue-trend")]
        public async Task<ActionResult<MonthlyRevenueTrendDto>> GetMonthlyRevenueTrend()
        {
            var data = await _mediator.Send(new GetMonthlyRevenueTrendQuery());
            return Ok(data);
        }

        /// <summary>
        /// Get top customers by revenue data for chart
        /// </summary>
        [HttpGet("top-customers")]
        public async Task<ActionResult<TopCustomersDto>> GetTopCustomers([FromQuery] int limit = 5)
        {
            var data = await _mediator.Send(new GetTopCustomersQuery(limit));
            return Ok(data);
        }

        /// <summary>
        /// Get revenue vs expenses data for chart
        /// </summary>
        [HttpGet("revenue-vs-expenses")]
        public async Task<ActionResult<RevenueVsExpensesDto>> GetRevenueVsExpenses()
        {
            var data = await _mediator.Send(new GetRevenueVsExpensesQuery());
            return Ok(data);
        }

        /// <summary>
        /// Get payment trend data for chart
        /// </summary>
        [HttpGet("payment-trend")]
        public async Task<ActionResult<PaymentTrendDto>> GetPaymentTrend([FromQuery] int months = 6)
        {
            var data = await _mediator.Send(new GetPaymentTrendQuery(months));
            return Ok(data);
        }

        /// <summary>
        /// Get account balance overview data for chart
        /// </summary>
        [HttpGet("account-balance-overview")]
        public async Task<ActionResult<AccountBalanceOverviewDto>> GetAccountBalanceOverview()
        {
            var data = await _mediator.Send(new GetAccountBalanceOverviewQuery());
            return Ok(data);
        }
    }
}
