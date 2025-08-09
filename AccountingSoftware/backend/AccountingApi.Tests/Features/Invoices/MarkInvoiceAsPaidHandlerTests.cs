using AccountingApi.DTOs;
using AccountingApi.Features.Invoices;
using AccountingApi.Infrastructure;
using AccountingApi.Models;
using AccountingApi.Services.AutomaticJournalEntryService;
using AccountingApi.Services.CurrentUserService;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace AccountingApi.Tests.Features.Invoices;

public class MarkInvoiceAsPaidHandlerTests
{
    private Mock<AccountingDbContext> _contextMock = null!;
    private Mock<IAutomaticJournalEntryService> _automaticJournalEntryServiceMock = null!;
    private Mock<ICurrentUserService> _currentUserServiceMock = null!;
    private MarkInvoiceAsPaidCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AccountingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _contextMock = new Mock<AccountingDbContext>(options);
        _automaticJournalEntryServiceMock = new Mock<IAutomaticJournalEntryService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        
        _handler = new MarkInvoiceAsPaidCommandHandler(
            _contextMock.Object,
            _automaticJournalEntryServiceMock.Object,
            _currentUserServiceMock.Object);
    }

    [Test]
    public async Task Handle_MarksInvoiceAsPaid_WhenValidRequest()
    {
        // Arrange
        const int invoiceId = 1;
        var paidDate = DateTime.UtcNow;
        const string paymentReference = "PAY-001";
        var command = new MarkInvoiceAsPaidCommand(invoiceId, paidDate, paymentReference);

        var customer = new Customer
        {
            Id = 1,
            CompanyName = "Test Customer",
            CustomerCode = "CUST-001"
        };

        var companyInfo = new CompanyInfo
        {
            Id = 1,
            CompanyName = "Test Company"
        };

        var invoice = new Invoice
        {
            Id = invoiceId,
            InvoiceNumber = "INV-2024-001",
            CustomerId = 1,
            Customer = customer,
            CompanyInfoId = 1,
            CompanyInfo = companyInfo,
            Status = InvoiceStatus.Sent,
            TotalAmount = 1000,
            InvoiceDate = DateTime.UtcNow.AddDays(-10),
            DueDate = DateTime.UtcNow.AddDays(20),
            Items = new List<InvoiceItem>
            {
                new() { Id = 1, InvoiceId = invoiceId, Description = "Service 1", Quantity = 1, UnitPrice = 1000, Amount = 1000, SortOrder = 1 }
            }
        };

        var mockInvoicesSet = new Mock<DbSet<Invoice>>();
        var invoicesQueryable = new List<Invoice> { invoice }.AsQueryable();
        
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.Provider).Returns(invoicesQueryable.Provider);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.Expression).Returns(invoicesQueryable.Expression);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.ElementType).Returns(invoicesQueryable.ElementType);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.GetEnumerator()).Returns(invoicesQueryable.GetEnumerator());

        _contextMock.Setup(c => c.Invoices).Returns(mockInvoicesSet.Object);
        _currentUserServiceMock.Setup(s => s.GetCurrentUserForAudit()).Returns("testuser");
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _automaticJournalEntryServiceMock.Setup(s => s.CreatePaymentJournalEntryAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(invoiceId));
        Assert.That(result.Status, Is.EqualTo(InvoiceStatus.Paid));
        Assert.That(result.StatusName, Is.EqualTo("Paid"));
        Assert.That(result.PaidDate, Is.EqualTo(paidDate));
        Assert.That(result.PaymentReference, Is.EqualTo(paymentReference));
        Assert.That(result.CustomerName, Is.EqualTo("Test Customer"));
        Assert.That(result.CompanyName, Is.EqualTo("Test Company"));
        
        // Verify invoice entity was updated
        Assert.That(invoice.Status, Is.EqualTo(InvoiceStatus.Paid));
        Assert.That(invoice.PaidDate, Is.EqualTo(paidDate));
        Assert.That(invoice.PaymentReference, Is.EqualTo(paymentReference));
        Assert.That(invoice.UpdatedBy, Is.EqualTo("testuser"));
        
        _currentUserServiceMock.Verify(s => s.GetCurrentUserForAudit(), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _automaticJournalEntryServiceMock.Verify(s => s.CreatePaymentJournalEntryAsync(invoice, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ThrowsException_WhenInvoiceNotFound()
    {
        // Arrange
        const int invoiceId = 999;
        var command = new MarkInvoiceAsPaidCommand(invoiceId, DateTime.UtcNow, "PAY-001");

        var mockInvoicesSet = new Mock<DbSet<Invoice>>();
        var invoicesQueryable = new List<Invoice>().AsQueryable();
        
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.Provider).Returns(invoicesQueryable.Provider);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.Expression).Returns(invoicesQueryable.Expression);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.ElementType).Returns(invoicesQueryable.ElementType);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.GetEnumerator()).Returns(invoicesQueryable.GetEnumerator());

        _contextMock.Setup(c => c.Invoices).Returns(mockInvoicesSet.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.That(ex.Message, Does.Contain($"Invoice with ID {invoiceId} not found"));
    }

    [Test]
    public async Task Handle_ThrowsException_WhenInvoiceAlreadyPaid()
    {
        // Arrange
        const int invoiceId = 1;
        var command = new MarkInvoiceAsPaidCommand(invoiceId, DateTime.UtcNow, "PAY-001");

        var invoice = new Invoice
        {
            Id = invoiceId,
            InvoiceNumber = "INV-2024-001",
            Status = InvoiceStatus.Paid,
            Customer = new Customer { CompanyName = "Test Customer" },
            CompanyInfo = new CompanyInfo { CompanyName = "Test Company" },
            Items = new List<InvoiceItem>()
        };

        var mockInvoicesSet = new Mock<DbSet<Invoice>>();
        var invoicesQueryable = new List<Invoice> { invoice }.AsQueryable();
        
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.Provider).Returns(invoicesQueryable.Provider);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.Expression).Returns(invoicesQueryable.Expression);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.ElementType).Returns(invoicesQueryable.ElementType);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.GetEnumerator()).Returns(invoicesQueryable.GetEnumerator());

        _contextMock.Setup(c => c.Invoices).Returns(mockInvoicesSet.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.That(ex.Message, Is.EqualTo("Invoice is already marked as paid."));
    }

    [Test]
    public async Task Handle_ThrowsException_WhenInvoiceIsCancelled()
    {
        // Arrange
        const int invoiceId = 1;
        var command = new MarkInvoiceAsPaidCommand(invoiceId, DateTime.UtcNow, "PAY-001");

        var invoice = new Invoice
        {
            Id = invoiceId,
            InvoiceNumber = "INV-2024-001",
            Status = InvoiceStatus.Cancelled,
            Customer = new Customer { CompanyName = "Test Customer" },
            CompanyInfo = new CompanyInfo { CompanyName = "Test Company" },
            Items = new List<InvoiceItem>()
        };

        var mockInvoicesSet = new Mock<DbSet<Invoice>>();
        var invoicesQueryable = new List<Invoice> { invoice }.AsQueryable();
        
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.Provider).Returns(invoicesQueryable.Provider);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.Expression).Returns(invoicesQueryable.Expression);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.ElementType).Returns(invoicesQueryable.ElementType);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.GetEnumerator()).Returns(invoicesQueryable.GetEnumerator());

        _contextMock.Setup(c => c.Invoices).Returns(mockInvoicesSet.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.That(ex.Message, Is.EqualTo("Cannot mark a cancelled invoice as paid."));
    }

    [Test]
    public async Task Handle_ContinuesWhenJournalEntryCreationFails()
    {
        // Arrange
        const int invoiceId = 1;
        var paidDate = DateTime.UtcNow;
        var command = new MarkInvoiceAsPaidCommand(invoiceId, paidDate, "PAY-001");

        var invoice = new Invoice
        {
            Id = invoiceId,
            InvoiceNumber = "INV-2024-001",
            Status = InvoiceStatus.Sent,
            Customer = new Customer { CompanyName = "Test Customer" },
            CompanyInfo = new CompanyInfo { CompanyName = "Test Company" },
            Items = new List<InvoiceItem>()
        };

        var mockInvoicesSet = new Mock<DbSet<Invoice>>();
        var invoicesQueryable = new List<Invoice> { invoice }.AsQueryable();
        
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.Provider).Returns(invoicesQueryable.Provider);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.Expression).Returns(invoicesQueryable.Expression);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.ElementType).Returns(invoicesQueryable.ElementType);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.GetEnumerator()).Returns(invoicesQueryable.GetEnumerator());

        _contextMock.Setup(c => c.Invoices).Returns(mockInvoicesSet.Object);
        _currentUserServiceMock.Setup(s => s.GetCurrentUserForAudit()).Returns("testuser");
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _automaticJournalEntryServiceMock.Setup(s => s.CreatePaymentJournalEntryAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Journal entry creation failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(InvoiceStatus.Paid));
        Assert.That(invoice.Status, Is.EqualTo(InvoiceStatus.Paid));
        
        // Verify that the payment was still processed despite journal entry failure
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _automaticJournalEntryServiceMock.Verify(s => s.CreatePaymentJournalEntryAsync(invoice, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_SetsAuditInformation_WhenMarkingAsPaid()
    {
        // Arrange
        const int invoiceId = 1;
        var command = new MarkInvoiceAsPaidCommand(invoiceId, DateTime.UtcNow, "PAY-001");

        var invoice = new Invoice
        {
            Id = invoiceId,
            Status = InvoiceStatus.Sent,
            UpdatedBy = "olduser",
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            Customer = new Customer { CompanyName = "Test Customer" },
            CompanyInfo = new CompanyInfo { CompanyName = "Test Company" },
            Items = new List<InvoiceItem>()
        };

        var mockInvoicesSet = new Mock<DbSet<Invoice>>();
        var invoicesQueryable = new List<Invoice> { invoice }.AsQueryable();
        
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.Provider).Returns(invoicesQueryable.Provider);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.Expression).Returns(invoicesQueryable.Expression);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.ElementType).Returns(invoicesQueryable.ElementType);
        mockInvoicesSet.As<IQueryable<Invoice>>().Setup(m => m.GetEnumerator()).Returns(invoicesQueryable.GetEnumerator());

        _contextMock.Setup(c => c.Invoices).Returns(mockInvoicesSet.Object);
        _currentUserServiceMock.Setup(s => s.GetCurrentUserForAudit()).Returns("newuser");
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _automaticJournalEntryServiceMock.Setup(s => s.CreatePaymentJournalEntryAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var beforeUpdate = DateTime.UtcNow;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(invoice.UpdatedBy, Is.EqualTo("newuser"));
        Assert.That(invoice.UpdatedAt, Is.GreaterThanOrEqualTo(beforeUpdate));
        Assert.That(invoice.UpdatedAt, Is.LessThanOrEqualTo(DateTime.UtcNow));
    }
}