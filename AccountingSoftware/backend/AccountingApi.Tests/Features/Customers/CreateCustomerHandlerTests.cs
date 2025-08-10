using AccountingApi.DTOs;
using AccountingApi.Features.Customers;
using AccountingApi.Models;
using AccountingApi.Services.NumberGenerationService;
using AccountingApi.Tests.TestHelpers;
using Moq;

namespace AccountingApi.Tests.Features.Customers;

public class CreateCustomerHandlerTests : BaseTestWithInMemoryDb
{
    private Mock<INumberGenerationService> _numberGenerationServiceMock = null!;
    private CreateCustomerCommandHandler _handler = null!;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        
        _numberGenerationServiceMock = new Mock<INumberGenerationService>();
        
        _handler = new CreateCustomerCommandHandler(
            Context,
            _numberGenerationServiceMock.Object,
            CurrentUserServiceMock.Object,
            CustomerMapper);
    }

    protected override void SeedTestData()
    {
        AddTestCustomers();
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
        
        _numberGenerationServiceMock.Setup(s => s.GenerateCustomerCodeAsync()).ReturnsAsync("CUST-001");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.CompanyName, Is.EqualTo("Test Company"));
        Assert.That(result.CustomerCode, Is.EqualTo("CUST-001"));
        Assert.That(result.Email, Is.EqualTo("john@testcompany.com"));
        
        // Verify the customer was created in the database
        var createdCustomer = Context.Customers.FirstOrDefault(c => c.CompanyName == "Test Company");
        Assert.That(createdCustomer, Is.Not.Null);
        Assert.That(createdCustomer.CustomerCode, Is.EqualTo("CUST-001"));
        Assert.That(createdCustomer.CreatedBy, Is.EqualTo("test-user-id"));
        Assert.That(createdCustomer.UpdatedBy, Is.EqualTo("test-user-id"));
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
        
        _numberGenerationServiceMock.Setup(s => s.GenerateCustomerCodeAsync()).ReturnsAsync("CUST-002");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var createdCustomer = Context.Customers.FirstOrDefault(c => c.CompanyName == "Audit Test Company");
        Assert.That(createdCustomer, Is.Not.Null);
        Assert.That(createdCustomer.CustomerCode, Is.EqualTo("CUST-002"));
        Assert.That(createdCustomer.CreatedBy, Is.EqualTo("test-user-id"));
        Assert.That(createdCustomer.UpdatedBy, Is.EqualTo("test-user-id"));
        Assert.That(createdCustomer.CreatedAt, Is.Not.Null);
        Assert.That(createdCustomer.UpdatedAt, Is.Not.Null);
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
        
        _numberGenerationServiceMock.Setup(s => s.GenerateCustomerCodeAsync()).ReturnsAsync("CUST-12345");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var createdCustomer = Context.Customers.FirstOrDefault(c => c.CompanyName == "Unique Code Company");
        Assert.That(createdCustomer, Is.Not.Null);
        Assert.That(createdCustomer.CustomerCode, Is.EqualTo("CUST-12345"));
        
        // Verify the number generation service was called
        _numberGenerationServiceMock.Verify(s => s.GenerateCustomerCodeAsync(), Times.Once);
    }

    [Test]
    public async Task Handle_StoresCustomerWithAllFields_WhenProvided()
    {
        // Arrange
        var createCustomerDto = new CreateCustomerDto
        {
            CompanyName = "Complete Test Company",
            ContactPersonName = "Complete User",
            Email = "complete@test.com",
            Phone = "555-1234",
            Address = "123 Complete St",
            City = "Complete City",
            State = "Complete State",
            PostalCode = "12345",
            Country = "Complete Country",
            Notes = "Complete test notes"
        };

        var command = new CreateCustomerCommand(createCustomerDto);
        
        _numberGenerationServiceMock.Setup(s => s.GenerateCustomerCodeAsync()).ReturnsAsync("CUST-FULL");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var createdCustomer = Context.Customers.FirstOrDefault(c => c.CompanyName == "Complete Test Company");
        Assert.That(createdCustomer, Is.Not.Null);
        Assert.That(createdCustomer.ContactPersonName, Is.EqualTo("Complete User"));
        Assert.That(createdCustomer.Email, Is.EqualTo("complete@test.com"));
        Assert.That(createdCustomer.Phone, Is.EqualTo("555-1234"));
        Assert.That(createdCustomer.Address, Is.EqualTo("123 Complete St"));
        Assert.That(createdCustomer.City, Is.EqualTo("Complete City"));
        Assert.That(createdCustomer.State, Is.EqualTo("Complete State"));
        Assert.That(createdCustomer.PostalCode, Is.EqualTo("12345"));
        Assert.That(createdCustomer.Country, Is.EqualTo("Complete Country"));
        Assert.That(createdCustomer.Notes, Is.EqualTo("Complete test notes"));
    }
}