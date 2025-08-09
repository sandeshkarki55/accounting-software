using AccountingApi.DTOs;
using AccountingApi.Features.Customers;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;
using AccountingApi.Models;
using AccountingApi.Services.CurrentUserService;
using AccountingApi.Services.NumberGenerationService;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace AccountingApi.Tests.Features.Customers;

public class CreateCustomerHandlerTests
{
    private Mock<AccountingDbContext> _contextMock = null!;
    private Mock<INumberGenerationService> _numberGenerationServiceMock = null!;
    private Mock<ICurrentUserService> _currentUserServiceMock = null!;
    private Mock<CustomerMapper> _mapperMock = null!;
    private CreateCustomerCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AccountingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _contextMock = new Mock<AccountingDbContext>(options);
        _numberGenerationServiceMock = new Mock<INumberGenerationService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _mapperMock = new Mock<CustomerMapper>();
        
        _handler = new CreateCustomerCommandHandler(
            _contextMock.Object,
            _numberGenerationServiceMock.Object,
            _currentUserServiceMock.Object,
            _mapperMock.Object);
    }

    [Test]
    public async Task Handle_CreatesCustomer_WhenValidRequest()
    {
        // Arrange
        var createCustomerDto = new CreateCustomerDto
        {
            CompanyName = "Test Company",
            ContactPersonName = "John Doe",
            Email = "john@testcompany.com",
            Phone = "123-456-7890",
            Address = "123 Main St",
            City = "Anytown",
            State = "State",
            PostalCode = "12345",
            Country = "Country",
            Notes = "Test notes"
        };

        var command = new CreateCustomerCommand(createCustomerDto);
        
        var customerEntity = new Customer
        {
            Id = 1,
            CompanyName = "Test Company",
            ContactPersonName = "John Doe",
            Email = "john@testcompany.com",
            Phone = "123-456-7890",
            Address = "123 Main St",
            City = "Anytown",
            State = "State",
            PostalCode = "12345",
            Country = "Country",
            Notes = "Test notes"
        };

        var expectedDto = new CustomerDto
        {
            Id = 1,
            CustomerCode = "CUST-001",
            CompanyName = "Test Company",
            ContactPersonName = "John Doe",
            Email = "john@testcompany.com",
            Phone = "123-456-7890",
            Address = "123 Main St",
            City = "Anytown",
            State = "State",
            PostalCode = "12345",
            Country = "Country",
            Notes = "Test notes",
            IsActive = true
        };

        var mockCustomersSet = new Mock<DbSet<Customer>>();
        
        _contextMock.Setup(c => c.Customers).Returns(mockCustomersSet.Object);
        _numberGenerationServiceMock.Setup(s => s.GenerateCustomerCodeAsync()).ReturnsAsync("CUST-001");
        _currentUserServiceMock.Setup(s => s.GetCurrentUserForAudit()).Returns("testuser");
        _mapperMock.Setup(m => m.ToEntity(createCustomerDto)).Returns(customerEntity);
        _mapperMock.Setup(m => m.ToDto(customerEntity)).Returns(expectedDto);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(expectedDto));
        Assert.That(customerEntity.CustomerCode, Is.EqualTo("CUST-001"));
        Assert.That(customerEntity.CreatedBy, Is.EqualTo("testuser"));
        Assert.That(customerEntity.UpdatedBy, Is.EqualTo("testuser"));
        
        _numberGenerationServiceMock.Verify(s => s.GenerateCustomerCodeAsync(), Times.Once);
        _currentUserServiceMock.Verify(s => s.GetCurrentUserForAudit(), Times.Once);
        _mapperMock.Verify(m => m.ToEntity(createCustomerDto), Times.Once);
        _mapperMock.Verify(m => m.ToDto(customerEntity), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockCustomersSet.Verify(s => s.Add(customerEntity), Times.Once);
    }

    [Test]
    public async Task Handle_SetsAuditInformation_WhenCreatingCustomer()
    {
        // Arrange
        var createCustomerDto = new CreateCustomerDto
        {
            CompanyName = "Audit Test Company",
            ContactPersonName = "Audit User",
            Email = "audit@test.com"
        };

        var command = new CreateCustomerCommand(createCustomerDto);
        
        var customerEntity = new Customer
        {
            CompanyName = "Audit Test Company",
            ContactPersonName = "Audit User",
            Email = "audit@test.com"
        };

        var expectedDto = new CustomerDto
        {
            Id = 1,
            CustomerCode = "CUST-002",
            CompanyName = "Audit Test Company",
            ContactPersonName = "Audit User",
            Email = "audit@test.com"
        };

        var mockCustomersSet = new Mock<DbSet<Customer>>();
        
        _contextMock.Setup(c => c.Customers).Returns(mockCustomersSet.Object);
        _numberGenerationServiceMock.Setup(s => s.GenerateCustomerCodeAsync()).ReturnsAsync("CUST-002");
        _currentUserServiceMock.Setup(s => s.GetCurrentUserForAudit()).Returns("audituser");
        _mapperMock.Setup(m => m.ToEntity(createCustomerDto)).Returns(customerEntity);
        _mapperMock.Setup(m => m.ToDto(customerEntity)).Returns(expectedDto);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(customerEntity.CustomerCode, Is.EqualTo("CUST-002"));
        Assert.That(customerEntity.CreatedBy, Is.EqualTo("audituser"));
        Assert.That(customerEntity.UpdatedBy, Is.EqualTo("audituser"));
        
        _currentUserServiceMock.Verify(s => s.GetCurrentUserForAudit(), Times.Once);
    }

    [Test]
    public async Task Handle_GeneratesUniqueCustomerCode_ForEachCustomer()
    {
        // Arrange
        var createCustomerDto = new CreateCustomerDto
        {
            CompanyName = "Unique Code Company",
            ContactPersonName = "Code User",
            Email = "code@test.com"
        };

        var command = new CreateCustomerCommand(createCustomerDto);
        
        var customerEntity = new Customer
        {
            CompanyName = "Unique Code Company",
            ContactPersonName = "Code User",
            Email = "code@test.com"
        };

        var expectedDto = new CustomerDto
        {
            CustomerCode = "CUST-12345",
            CompanyName = "Unique Code Company",
            ContactPersonName = "Code User",
            Email = "code@test.com"
        };

        var mockCustomersSet = new Mock<DbSet<Customer>>();
        
        _contextMock.Setup(c => c.Customers).Returns(mockCustomersSet.Object);
        _numberGenerationServiceMock.Setup(s => s.GenerateCustomerCodeAsync()).ReturnsAsync("CUST-12345");
        _currentUserServiceMock.Setup(s => s.GetCurrentUserForAudit()).Returns("testuser");
        _mapperMock.Setup(m => m.ToEntity(createCustomerDto)).Returns(customerEntity);
        _mapperMock.Setup(m => m.ToDto(customerEntity)).Returns(expectedDto);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(customerEntity.CustomerCode, Is.EqualTo("CUST-12345"));
        
        _numberGenerationServiceMock.Verify(s => s.GenerateCustomerCodeAsync(), Times.Once);
    }

    [Test]
    public async Task Handle_CallsMapperCorrectly_WithEntityAndDto()
    {
        // Arrange
        var createCustomerDto = new CreateCustomerDto
        {
            CompanyName = "Mapper Test Company",
            ContactPersonName = "Mapper User",
            Email = "mapper@test.com"
        };

        var command = new CreateCustomerCommand(createCustomerDto);
        
        var customerEntity = new Customer();
        var expectedDto = new CustomerDto();

        var mockCustomersSet = new Mock<DbSet<Customer>>();
        
        _contextMock.Setup(c => c.Customers).Returns(mockCustomersSet.Object);
        _numberGenerationServiceMock.Setup(s => s.GenerateCustomerCodeAsync()).ReturnsAsync("CUST-001");
        _currentUserServiceMock.Setup(s => s.GetCurrentUserForAudit()).Returns("testuser");
        _mapperMock.Setup(m => m.ToEntity(createCustomerDto)).Returns(customerEntity);
        _mapperMock.Setup(m => m.ToDto(customerEntity)).Returns(expectedDto);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mapperMock.Verify(m => m.ToEntity(
            It.Is<CreateCustomerDto>(dto => dto == createCustomerDto)), Times.Once);
        _mapperMock.Verify(m => m.ToDto(
            It.Is<Customer>(c => c == customerEntity)), Times.Once);
    }
}