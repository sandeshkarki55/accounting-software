using AccountingApi.Controllers;
using AccountingApi.DTOs;
using AccountingApi.Features.Invoices;

using Microsoft.AspNetCore.Mvc;

using Moq;

using MyMediator;

namespace AccountingApi.Tests.Controllers;

public class InvoicesControllerTests
{
    private Mock<IMediator> _mediatorMock = null!;
    private InvoicesController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
        _controller = new InvoicesController(_mediatorMock.Object);
    }

    [Test]
    public async Task GetInvoices_ReturnsOk_WithPagedResults()
    {
        // Arrange
        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var sorting = new SortingParams { SortField = "InvoiceNumber", SortOrder = "desc" };
        var filtering = new InvoiceFilteringParams { SearchTerm = "INV-2024" };
        
        var expectedResult = new PagedResult<InvoiceDto>
        {
            Items = new List<InvoiceDto>
            {
                new() { Id = 1, InvoiceNumber = "INV-2024-001", TotalAmount = 1000 },
                new() { Id = 2, InvoiceNumber = "INV-2024-002", TotalAmount = 1500 }
            },
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllInvoicesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetInvoices(pagination, sorting, filtering);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(expectedResult));
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<GetAllInvoicesQuery>(q => 
                q.Pagination == pagination && 
                q.Sorting == sorting && 
                q.Filtering == filtering), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetInvoice_ReturnsOk_WhenInvoiceExists()
    {
        // Arrange
        const int invoiceId = 1;
        var expectedInvoice = new InvoiceDto 
        { 
            Id = invoiceId, 
            InvoiceNumber = "INV-2024-001",
            TotalAmount = 1000,
            CustomerId = 1,
            CustomerName = "Test Customer"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetInvoiceByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedInvoice);

        // Act
        var result = await _controller.GetInvoice(invoiceId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(expectedInvoice));
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<GetInvoiceByIdQuery>(q => q.Id == invoiceId), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetInvoice_ReturnsNotFound_WhenInvoiceDoesNotExist()
    {
        // Arrange
        const int invoiceId = 99;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetInvoiceByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((InvoiceDto?)null);

        // Act
        var result = await _controller.GetInvoice(invoiceId);

        // Assert
        var notFoundResult = result.Result as NotFoundResult;
        Assert.That(notFoundResult, Is.Not.Null);
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<GetInvoiceByIdQuery>(q => q.Id == invoiceId), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CreateInvoice_ReturnsCreatedAtAction_WhenSuccessful()
    {
        // Arrange
        var createInvoiceDto = new CreateInvoiceDto
        {
            CustomerId = 1,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Items = new List<InvoiceItemDto>
            {
                new() { Description = "Service 1", Quantity = 1, Rate = 1000 }
            }
        };

        var createdInvoice = new InvoiceDto
        {
            Id = 1,
            InvoiceNumber = "INV-2024-001",
            CustomerId = 1,
            CustomerName = "Test Customer",
            InvoiceDate = createInvoiceDto.InvoiceDate,
            DueDate = createInvoiceDto.DueDate,
            TotalAmount = 1000,
            Status = InvoiceStatus.Draft
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateInvoiceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdInvoice);

        // Act
        var result = await _controller.CreateInvoice(createInvoiceDto);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.That(createdResult.ActionName, Is.EqualTo(nameof(InvoicesController.GetInvoice)));
        Assert.That(createdResult.RouteValues!["id"], Is.EqualTo(createdInvoice.Id));
        Assert.That(createdResult.Value, Is.EqualTo(createdInvoice));
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<CreateInvoiceCommand>(c => c.Invoice == createInvoiceDto), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task MarkInvoiceAsPaid_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        const int invoiceId = 1;
        var markAsPaidDto = new MarkInvoiceAsPaidDto
        {
            PaidDate = DateTime.UtcNow,
            PaymentReference = "PAY-001"
        };

        var paidInvoice = new InvoiceDto
        {
            Id = invoiceId,
            InvoiceNumber = "INV-2024-001",
            Status = InvoiceStatus.Paid,
            PaidDate = markAsPaidDto.PaidDate,
            PaymentReference = markAsPaidDto.PaymentReference
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<MarkInvoiceAsPaidCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paidInvoice);

        // Act
        var result = await _controller.MarkInvoiceAsPaid(invoiceId, markAsPaidDto);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(paidInvoice));
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<MarkInvoiceAsPaidCommand>(c => 
                c.Id == invoiceId && 
                c.PaidDate == markAsPaidDto.PaidDate && 
                c.PaymentReference == markAsPaidDto.PaymentReference), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteInvoice_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        const int invoiceId = 1;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteInvoiceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteInvoice(invoiceId);

        // Assert
        var noContentResult = result as NoContentResult;
        Assert.That(noContentResult, Is.Not.Null);
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<DeleteInvoiceCommand>(c => c.Id == invoiceId), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteInvoice_ReturnsNotFound_WhenInvoiceDoesNotExist()
    {
        // Arrange
        const int invoiceId = 99;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteInvoiceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteInvoice(invoiceId);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        
        var response = notFoundResult.Value;
        Assert.That(response, Is.Not.Null);
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<DeleteInvoiceCommand>(c => c.Id == invoiceId), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteInvoiceItem_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        const int invoiceItemId = 1;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteInvoiceItemCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteInvoiceItem(invoiceItemId);

        // Assert
        var noContentResult = result as NoContentResult;
        Assert.That(noContentResult, Is.Not.Null);
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<DeleteInvoiceItemCommand>(c => c.Id == invoiceItemId), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteInvoiceItem_ReturnsNotFound_WhenInvoiceItemDoesNotExist()
    {
        // Arrange
        const int invoiceItemId = 99;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteInvoiceItemCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteInvoiceItem(invoiceItemId);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        
        var response = notFoundResult.Value;
        Assert.That(response, Is.Not.Null);
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<DeleteInvoiceItemCommand>(c => c.Id == invoiceItemId), 
            It.IsAny<CancellationToken>()), Times.Once);
    }
}