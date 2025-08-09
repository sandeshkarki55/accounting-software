using AccountingApi.DTOs.Dashboard;
using AccountingApi.Features.Dashboard.Handlers;
using AccountingApi.Infrastructure;
using AccountingApi.Models;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace AccountingApi.Tests.Features.Dashboard;

public class GetDashboardStatsHandlerTests
{
    private Mock<AccountingDbContext> _contextMock = null!;
    private GetDashboardStatsHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AccountingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _contextMock = new Mock<AccountingDbContext>(options);
        _handler = new GetDashboardStatsHandler(_contextMock.Object);
    }

    [Test]
    public async Task Handle_ReturnsCorrectStats_WithValidData()
    {
        // Arrange
        var query = new GetDashboardStatsQuery();
        var currentDate = DateTime.Now;
        
        var invoices = new List<Invoice>
        {
            new() { Id = 1, TotalAmount = 1000, Status = InvoiceStatus.Paid, InvoiceDate = currentDate, DueDate = currentDate.AddDays(-5) },
            new() { Id = 2, TotalAmount = 2000, Status = InvoiceStatus.Sent, InvoiceDate = currentDate, DueDate = currentDate.AddDays(10) },
            new() { Id = 3, TotalAmount = 1500, Status = InvoiceStatus.Paid, InvoiceDate = currentDate.AddMonths(-1), DueDate = currentDate.AddMonths(-1).AddDays(30) },
            new() { Id = 4, TotalAmount = 500, Status = InvoiceStatus.Sent, InvoiceDate = currentDate, DueDate = currentDate.AddDays(-2) } // Overdue
        };

        var customers = new List<Customer>
        {
            new() { Id = 1, CompanyName = "Company 1", IsActive = true },
            new() { Id = 2, CompanyName = "Company 2", IsActive = true },
            new() { Id = 3, CompanyName = "Company 3", IsActive = false }
        };

        var mockInvoicesSet = new Mock<DbSet<Invoice>>();
        var invoicesQueryable = invoices.AsQueryable();
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.Provider).Returns(invoicesQueryable.Provider);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.Expression).Returns(invoicesQueryable.Expression);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.ElementType).Returns(invoicesQueryable.ElementType);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.GetEnumerator()).Returns(invoicesQueryable.GetEnumerator());

        var mockCustomersSet = new Mock<DbSet<Customer>>();
        var customersQueryable = customers.AsQueryable();
        mockCustomersSet.As<IQueryable<Customer>>().Setup(m => m.Provider).Returns(customersQueryable.Provider);
        mockCustomersSet.As<IQueryable<Customer>>().Setup(m => m.Expression).Returns(customersQueryable.Expression);
        mockCustomersSet.As<IQueryable<Customer>>().Setup(m => m.ElementType).Returns(customersQueryable.ElementType);
        mockCustomersSet.As<IQueryable<Customer>>().Setup(m => m.GetEnumerator()).Returns(customersQueryable.GetEnumerator());

        _contextMock.Setup(c => c.Invoices).Returns(mockInvoicesSet.Object);
        _contextMock.Setup(c => c.Customers).Returns(mockCustomersSet.Object);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.TotalRevenue, Is.EqualTo(2500)); // Two paid invoices: 1000 + 1500
        Assert.That(result.OutstandingAmount, Is.EqualTo(2500)); // Two unpaid invoices: 2000 + 500
        Assert.That(result.OverdueAmount, Is.EqualTo(500)); // One overdue invoice
        Assert.That(result.ActiveCustomers, Is.EqualTo(2)); // Two active customers
        Assert.That(result.AverageInvoiceValue, Is.EqualTo(1250)); // (1000+2000+1500+500)/4
        Assert.That(result.TotalInvoices, Is.EqualTo(4));
        Assert.That(result.PaidInvoices, Is.EqualTo(2));
        Assert.That(result.PaymentRate, Is.EqualTo(50)); // 2/4 * 100
    }

    [Test]
    public async Task Handle_ReturnsZeroStats_WhenNoData()
    {
        // Arrange
        var query = new GetDashboardStatsQuery();
        
        var invoices = new List<Invoice>();
        var customers = new List<Customer>();

        var mockInvoicesSet = new Mock<DbSet<Invoice>>();
        var invoicesQueryable = invoices.AsQueryable();
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.Provider).Returns(invoicesQueryable.Provider);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.Expression).Returns(invoicesQueryable.Expression);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.ElementType).Returns(invoicesQueryable.ElementType);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.GetEnumerator()).Returns(invoicesQueryable.GetEnumerator());

        var mockCustomersSet = new Mock<DbSet<Customer>>();
        var customersQueryable = customers.AsQueryable();
        mockCustomersSet.As<IQueryable<Customer>>().Setup(m => m.Provider).Returns(customersQueryable.Provider);
        mockCustomersSet.As<IQueryable<Customer>>().Setup(m => m.Expression).Returns(customersQueryable.Expression);
        mockCustomersSet.As<IQueryable<Customer>>().Setup(m => m.ElementType).Returns(customersQueryable.ElementType);
        mockCustomersSet.As<IQueryable<Customer>>().Setup(m => m.GetEnumerator()).Returns(customersQueryable.GetEnumerator());

        _contextMock.Setup(c => c.Invoices).Returns(mockInvoicesSet.Object);
        _contextMock.Setup(c => c.Customers).Returns(mockCustomersSet.Object);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.TotalRevenue, Is.EqualTo(0));
        Assert.That(result.OutstandingAmount, Is.EqualTo(0));
        Assert.That(result.OverdueAmount, Is.EqualTo(0));
        Assert.That(result.ActiveCustomers, Is.EqualTo(0));
        Assert.That(result.AverageInvoiceValue, Is.EqualTo(0));
        Assert.That(result.TotalInvoices, Is.EqualTo(0));
        Assert.That(result.PaidInvoices, Is.EqualTo(0));
        Assert.That(result.PaymentRate, Is.EqualTo(0));
    }

    [Test]
    public async Task Handle_HandlesInactiveCustomers_Correctly()
    {
        // Arrange
        var query = new GetDashboardStatsQuery();
        
        var invoices = new List<Invoice>
        {
            new() { Id = 1, TotalAmount = 1000, Status = InvoiceStatus.Paid, InvoiceDate = DateTime.Now, DueDate = DateTime.Now.AddDays(30) }
        };

        var customers = new List<Customer>
        {
            new() { Id = 1, CompanyName = "Active Company", IsActive = true },
            new() { Id = 2, CompanyName = "Inactive Company 1", IsActive = false },
            new() { Id = 3, CompanyName = "Inactive Company 2", IsActive = false }
        };

        var mockInvoicesSet = new Mock<DbSet<Invoice>>();
        var invoicesQueryable = invoices.AsQueryable();
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.Provider).Returns(invoicesQueryable.Provider);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.Expression).Returns(invoicesQueryable.Expression);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.ElementType).Returns(invoicesQueryable.ElementType);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.GetEnumerator()).Returns(invoicesQueryable.GetEnumerator());

        var mockCustomersSet = new Mock<DbSet<Customer>>();
        var customersQueryable = customers.AsQueryable();
        mockCustomersSet.As<IQueryable<Customer>>().Setup(m => m.Provider).Returns(customersQueryable.Provider);
        mockCustomersSet.As<IQueryable<Customer>>().Setup(m => m.Expression).Returns(customersQueryable.Expression);
        mockCustomersSet.As<IQueryable<Customer>>().Setup(m => m.ElementType).Returns(customersQueryable.ElementType);
        mockCustomersSet.As<IQueryable<Customer>>().Setup(m => m.GetEnumerator()).Returns(customersQueryable.GetEnumerator());

        _contextMock.Setup(c => c.Invoices).Returns(mockInvoicesSet.Object);
        _contextMock.Setup(c => c.Customers).Returns(mockCustomersSet.Object);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result.ActiveCustomers, Is.EqualTo(1)); // Only one active customer
    }

    [Test]
    public async Task Handle_CalculatesOverdueInvoices_Correctly()
    {
        // Arrange
        var query = new GetDashboardStatsQuery();
        var pastDate = DateTime.Now.AddDays(-10);
        var futureDate = DateTime.Now.AddDays(10);
        
        var invoices = new List<Invoice>
        {
            new() { Id = 1, TotalAmount = 1000, Status = InvoiceStatus.Sent, DueDate = pastDate }, // Overdue
            new() { Id = 2, TotalAmount = 2000, Status = InvoiceStatus.Sent, DueDate = futureDate }, // Not overdue
            new() { Id = 3, TotalAmount = 1500, Status = InvoiceStatus.Paid, DueDate = pastDate }, // Paid, not overdue
            new() { Id = 4, TotalAmount = 500, Status = InvoiceStatus.Cancelled, DueDate = pastDate } // Cancelled, not overdue
        };

        var customers = new List<Customer>();

        var mockInvoicesSet = new Mock<DbSet<Invoice>>();
        var invoicesQueryable = invoices.AsQueryable();
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.Provider).Returns(invoicesQueryable.Provider);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.Expression).Returns(invoicesQueryable.Expression);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.ElementType).Returns(invoicesQueryable.ElementType);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.GetEnumerator()).Returns(invoicesQueryable.GetEnumerator());

        var mockCustomersSet = new Mock<DbSet<Customer>>();
        var customersQueryable = customers.AsQueryable();
        mockCustomersSet.As<IQueryable<Customer>>().Setup(m => m.Provider).Returns(customersQueryable.Provider);
        mockCustomersSet.As<IQueryable<Customer>>().Setup(m => m.Expression).Returns(customersQueryable.Expression);
        mockCustomersSet.As<IQueryable<Customer>>().Setup(m => m.ElementType).Returns(customersQueryable.ElementType);
        mockCustomersSet.As<IQueryable<Customer>>().Setup(m => m.GetEnumerator()).Returns(customersQueryable.GetEnumerator());

        _contextMock.Setup(c => c.Invoices).Returns(mockInvoicesSet.Object);
        _contextMock.Setup(c => c.Customers).Returns(mockCustomersSet.Object);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result.OverdueAmount, Is.EqualTo(1000)); // Only the first invoice is overdue and unpaid
        Assert.That(result.OutstandingAmount, Is.EqualTo(3000)); // First two invoices are outstanding: 1000 + 2000
    }
}