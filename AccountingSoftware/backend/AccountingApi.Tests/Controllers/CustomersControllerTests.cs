using AccountingApi.Controllers;
using AccountingApi.DTOs;
using AccountingApi.Features.Customers;

using Microsoft.AspNetCore.Mvc;

using Moq;

using MyMediator;

namespace AccountingApi.Tests.Controllers;

public class CustomersControllerTests
{
    private Mock<IMediator> _mediatorMock = null!;
    private CustomersController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
        _controller = new CustomersController(_mediatorMock.Object);
    }

    [Test]
    public async Task GetCustomers_ReturnsOk_WithPagedResults()
    {
        // Arrange
        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var sorting = new SortingParams { SortField = "CompanyName", SortOrder = "asc" };
        var filtering = new CustomerFilteringParams { SearchTerm = "test" };
        
        var expectedResult = new PagedResult<CustomerDto>
        {
            Items = new List<CustomerDto>
            {
                new() { Id = 1, CompanyName = "Test Company", Email = "test@company.com" },
                new() { Id = 2, CompanyName = "Another Company", Email = "info@another.com" }
            },
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllCustomersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetCustomers(pagination, sorting, filtering);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(expectedResult));
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<GetAllCustomersQuery>(q => 
                q.Pagination == pagination && 
                q.Sorting == sorting && 
                q.Filtering == filtering), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetCustomer_ReturnsOk_WhenCustomerExists()
    {
        // Arrange
        const int customerId = 1;
        var expectedCustomer = new CustomerDto 
        { 
            Id = customerId, 
            CompanyName = "Test Company", 
            Email = "test@company.com",
            ContactPersonName = "John Doe"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetCustomerByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCustomer);

        // Act
        var result = await _controller.GetCustomer(customerId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(expectedCustomer));
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<GetCustomerByIdQuery>(q => q.Id == customerId), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetCustomer_ReturnsNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        const int customerId = 99;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetCustomerByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerDto?)null);

        // Act
        var result = await _controller.GetCustomer(customerId);

        // Assert
        var notFoundResult = result.Result as NotFoundResult;
        Assert.That(notFoundResult, Is.Not.Null);
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<GetCustomerByIdQuery>(q => q.Id == customerId), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CreateCustomer_ReturnsCreatedAtAction_WhenSuccessful()
    {
        // Arrange
        var createCustomerDto = new CreateCustomerDto
        {
            CompanyName = "New Company",
            ContactPersonName = "Jane Smith",
            Email = "jane@newcompany.com",
            Phone = "123-456-7890",
            Address = "123 Main St",
            City = "Anytown",
            State = "State",
            PostalCode = "12345",
            Country = "Country"
        };

        var createdCustomer = new CustomerDto
        {
            Id = 1,
            CustomerCode = "CUST-001",
            CompanyName = "New Company",
            ContactPersonName = "Jane Smith",
            Email = "jane@newcompany.com",
            Phone = "123-456-7890",
            Address = "123 Main St",
            City = "Anytown",
            State = "State",
            PostalCode = "12345",
            Country = "Country",
            IsActive = true
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateCustomerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdCustomer);

        // Act
        var result = await _controller.CreateCustomer(createCustomerDto);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.That(createdResult.ActionName, Is.EqualTo(nameof(CustomersController.GetCustomer)));
        Assert.That(createdResult.RouteValues!["id"], Is.EqualTo(createdCustomer.Id));
        Assert.That(createdResult.Value, Is.EqualTo(createdCustomer));
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<CreateCustomerCommand>(c => c.Customer == createCustomerDto), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateCustomer_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        const int customerId = 1;
        var updateCustomerDto = new UpdateCustomerDto
        {
            CompanyName = "Updated Company",
            ContactPersonName = "Updated Contact",
            Email = "updated@company.com",
            IsActive = false
        };

        var updatedCustomer = new CustomerDto
        {
            Id = customerId,
            CustomerCode = "CUST-001",
            CompanyName = "Updated Company",
            ContactPersonName = "Updated Contact",
            Email = "updated@company.com",
            IsActive = false
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateCustomerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedCustomer);

        // Act
        var result = await _controller.UpdateCustomer(customerId, updateCustomerDto);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(updatedCustomer));
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<UpdateCustomerCommand>(c => c.Id == customerId && c.Customer == updateCustomerDto), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateCustomer_ReturnsNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        const int customerId = 99;
        var updateCustomerDto = new UpdateCustomerDto
        {
            CompanyName = "Updated Company",
            ContactPersonName = "Updated Contact",
            Email = "updated@company.com"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateCustomerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerDto?)null);

        // Act
        var result = await _controller.UpdateCustomer(customerId, updateCustomerDto);

        // Assert
        var notFoundResult = result as NotFoundResult;
        Assert.That(notFoundResult, Is.Not.Null);
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<UpdateCustomerCommand>(c => c.Id == customerId && c.Customer == updateCustomerDto), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteCustomer_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        const int customerId = 1;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteCustomerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteCustomer(customerId);

        // Assert
        var noContentResult = result as NoContentResult;
        Assert.That(noContentResult, Is.Not.Null);
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<DeleteCustomerCommand>(c => c.Id == customerId), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteCustomer_ReturnsNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        const int customerId = 99;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteCustomerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteCustomer(customerId);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        
        var response = notFoundResult.Value;
        Assert.That(response, Is.Not.Null);
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<DeleteCustomerCommand>(c => c.Id == customerId), 
            It.IsAny<CancellationToken>()), Times.Once);
    }
}