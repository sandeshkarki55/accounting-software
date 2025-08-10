using AccountingApi.Controllers;
using AccountingApi.DTOs.Dashboard;
using AccountingApi.Features.Dashboard.Handlers;

using Microsoft.AspNetCore.Mvc;

using Moq;

using MyMediator;

namespace AccountingApi.Tests.Controllers;

public class DashboardControllerTests
{
    private Mock<IMediator> _mediatorMock = null!;
    private DashboardController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
        _controller = new DashboardController(_mediatorMock.Object);
    }

    [Test]
    public async Task GetDashboardStats_ReturnsOk_WithStats()
    {
        // Arrange
        var expectedStats = new DashboardStatsDto
        {
            TotalRevenue = 50000,
            OutstandingInvoices = 30000,
            ActiveCustomers = 150,
            TotalInvoiceCount = 300,
            PaidInvoicesCount = 275,
            OverdueAmount = 25000
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetDashboardStatsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStats);

        // Act
        var result = await _controller.GetDashboardStats();

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(expectedStats));
        
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetDashboardStatsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetInvoiceStatusDistribution_ReturnsOk_WithDistributionData()
    {
        // Arrange
        var expectedData = new InvoiceStatusDistributionDto
        {
            Labels = new List<string> { "Paid", "Pending", "Overdue", "Draft" },
            Data = new List<int> { 200, 50, 15, 10 },
            BackgroundColors = new List<string> { "#28a745", "#ffc107", "#dc3545", "#6c757d" }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetInvoiceStatusDistributionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        // Act
        var result = await _controller.GetInvoiceStatusDistribution();

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(expectedData));
        
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetInvoiceStatusDistributionQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetMonthlyRevenueTrend_ReturnsOk_WithTrendData()
    {
        // Arrange
        var expectedData = new MonthlyRevenueTrendDto
        {
            Labels = new List<string> { "January", "February", "March" },
            Data = new List<decimal> { 10000, 12000, 15000 }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetMonthlyRevenueTrendQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        // Act
        var result = await _controller.GetMonthlyRevenueTrend();

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(expectedData));
        
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetMonthlyRevenueTrendQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetTopCustomers_ReturnsOk_WithDefaultLimit()
    {
        // Arrange
        const int defaultLimit = 5;
        var expectedData = new TopCustomersDto
        {
            Labels = new List<string> { "Customer 1", "Customer 2", "Customer 3" },
            Data = new List<decimal> { 25000, 20000, 15000 },
            BackgroundColors = new List<string> { "#ff6384", "#36a2eb", "#ffce56" }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetTopCustomersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        // Act
        var result = await _controller.GetTopCustomers();

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(expectedData));
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<GetTopCustomersQuery>(q => q.Limit == defaultLimit), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetTopCustomers_ReturnsOk_WithCustomLimit()
    {
        // Arrange
        const int customLimit = 10;
        var expectedData = new TopCustomersDto
        {
            Labels = new List<string> { "Customer 1" },
            Data = new List<decimal> { 25000 },
            BackgroundColors = new List<string> { "#ff6384" }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetTopCustomersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        // Act
        var result = await _controller.GetTopCustomers(customLimit);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(expectedData));
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<GetTopCustomersQuery>(q => q.Limit == customLimit), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetRevenueVsExpenses_ReturnsOk_WithComparisonData()
    {
        // Arrange
        var expectedData = new RevenueVsExpensesDto
        {
            Labels = new List<string> { "January", "February" },
            RevenueData = new List<decimal> { 8000, 9000 },
            ExpensesData = new List<decimal> { 6000, 6500 }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetRevenueVsExpensesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        // Act
        var result = await _controller.GetRevenueVsExpenses();

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(expectedData));
        
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetRevenueVsExpensesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetPaymentTrend_ReturnsOk_WithDefaultMonths()
    {
        // Arrange
        const int defaultMonths = 6;
        var expectedData = new PaymentTrendDto
        {
            Labels = new List<string> { "January", "February" },
            Data = new List<decimal> { 15000, 18000 }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPaymentTrendQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        // Act
        var result = await _controller.GetPaymentTrend();

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(expectedData));
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<GetPaymentTrendQuery>(q => q.Months == defaultMonths), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetPaymentTrend_ReturnsOk_WithCustomMonths()
    {
        // Arrange
        const int customMonths = 12;
        var expectedData = new PaymentTrendDto
        {
            Labels = new List<string> { "January" },
            Data = new List<decimal> { 15000 }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPaymentTrendQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        // Act
        var result = await _controller.GetPaymentTrend(customMonths);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(expectedData));
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<GetPaymentTrendQuery>(q => q.Months == customMonths), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetAccountBalanceOverview_ReturnsOk_WithBalanceData()
    {
        // Arrange
        var expectedData = new AccountBalanceOverviewDto
        {
            Labels = new List<string> { "Cash", "Accounts Receivable", "Accounts Payable" },
            Data = new List<decimal> { 50000, 25000, 15000 },
            BackgroundColors = new List<string> { "#28a745", "#17a2b8", "#dc3545" }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAccountBalanceOverviewQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        // Act
        var result = await _controller.GetAccountBalanceOverview();

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(expectedData));
        
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetAccountBalanceOverviewQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}