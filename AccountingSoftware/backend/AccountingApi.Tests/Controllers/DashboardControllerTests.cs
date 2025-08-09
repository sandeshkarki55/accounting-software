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
            TotalExpenses = 30000,
            NetProfit = 20000,
            TotalCustomers = 150,
            TotalInvoices = 300,
            PendingInvoices = 25
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
            PaidInvoices = 200,
            PendingInvoices = 50,
            OverdueInvoices = 15,
            DraftInvoices = 10
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
            MonthlyData = new List<MonthlyRevenueData>
            {
                new() { Month = "January", Revenue = 10000 },
                new() { Month = "February", Revenue = 12000 },
                new() { Month = "March", Revenue = 15000 }
            }
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
            Customers = new List<TopCustomerData>
            {
                new() { CustomerName = "Customer 1", Revenue = 25000 },
                new() { CustomerName = "Customer 2", Revenue = 20000 },
                new() { CustomerName = "Customer 3", Revenue = 15000 }
            }
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
            Customers = new List<TopCustomerData>
            {
                new() { CustomerName = "Customer 1", Revenue = 25000 }
            }
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
            TotalRevenue = 100000,
            TotalExpenses = 75000,
            NetProfit = 25000,
            MonthlyComparison = new List<MonthlyComparisonData>
            {
                new() { Month = "January", Revenue = 8000, Expenses = 6000 },
                new() { Month = "February", Revenue = 9000, Expenses = 6500 }
            }
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
            PaymentData = new List<PaymentTrendData>
            {
                new() { Month = "January", TotalPayments = 15000, PaymentCount = 25 },
                new() { Month = "February", TotalPayments = 18000, PaymentCount = 30 }
            }
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
            PaymentData = new List<PaymentTrendData>
            {
                new() { Month = "January", TotalPayments = 15000, PaymentCount = 25 }
            }
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
            AccountBalances = new List<AccountBalanceData>
            {
                new() { AccountName = "Cash", AccountType = "Asset", Balance = 50000 },
                new() { AccountName = "Accounts Receivable", AccountType = "Asset", Balance = 25000 },
                new() { AccountName = "Accounts Payable", AccountType = "Liability", Balance = -15000 }
            },
            TotalAssets = 75000,
            TotalLiabilities = 15000,
            TotalEquity = 60000
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