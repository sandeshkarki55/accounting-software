using AccountingApi.DTOs;
using AccountingApi.Features.Accounts;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;
using AccountingApi.Models;
using AccountingApi.Services.CurrentUserService;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

namespace AccountingApi.Tests.Features.Accounts;

public class CreateAccountHandlerTests
{
    private Mock<AccountingDbContext> _contextMock = null!;
    private Mock<AccountMapper> _mapperMock = null!;
    private Mock<ICurrentUserService> _currentUserServiceMock = null!;
    private Mock<ILogger<CreateAccountCommandHandler>> _loggerMock = null!;
    private CreateAccountCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AccountingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _contextMock = new Mock<AccountingDbContext>(options);
        _mapperMock = new Mock<AccountMapper>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<CreateAccountCommandHandler>>();
        
        _handler = new CreateAccountCommandHandler(
            _contextMock.Object,
            _mapperMock.Object,
            _currentUserServiceMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task Handle_CreatesAccount_WhenValidRequest()
    {
        // Arrange
        var createAccountDto = new CreateAccountDto
        {
            AccountCode = "1000",
            AccountName = "Cash",
            AccountType = AccountType.Asset,
            Description = "Cash account"
        };

        var command = new CreateAccountCommand(createAccountDto);
        
        var accountEntity = new Account
        {
            Id = 1,
            AccountCode = "1000",
            AccountName = "Cash",
            AccountType = AccountType.Asset,
            Description = "Cash account"
        };

        var expectedDto = new AccountDto
        {
            Id = 1,
            AccountCode = "1000",
            AccountName = "Cash",
            AccountType = AccountType.Asset,
            Description = "Cash account"
        };

        var mockAccountsSet = new Mock<DbSet<Account>>();
        var accountsList = new List<Account>().AsQueryable();
        
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Provider).Returns(accountsList.Provider);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Expression).Returns(accountsList.Expression);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.ElementType).Returns(accountsList.ElementType);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.GetEnumerator()).Returns(accountsList.GetEnumerator());

        _contextMock.Setup(c => c.Accounts).Returns(mockAccountsSet.Object);
        _mapperMock.Setup(m => m.ToEntity(createAccountDto)).Returns(accountEntity);
        _mapperMock.Setup(m => m.ToDto(accountEntity)).Returns(expectedDto);
        _currentUserServiceMock.Setup(s => s.GetCurrentUserForAudit()).Returns("testuser");
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(expectedDto));
        _mapperMock.Verify(m => m.ToEntity(createAccountDto), Times.Once);
        _mapperMock.Verify(m => m.ToDto(accountEntity), Times.Once);
        _currentUserServiceMock.Verify(s => s.GetCurrentUserForAudit(), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockAccountsSet.Verify(s => s.Add(accountEntity), Times.Once);
    }

    [Test]
    public async Task Handle_ThrowsException_WhenAccountCodeAlreadyExists()
    {
        // Arrange
        var createAccountDto = new CreateAccountDto
        {
            AccountCode = "1000",
            AccountName = "Cash",
            AccountType = AccountType.Asset,
            Description = "Cash account"
        };

        var command = new CreateAccountCommand(createAccountDto);

        var existingAccount = new Account
        {
            Id = 1,
            AccountCode = "1000",
            AccountName = "Existing Cash",
            AccountType = AccountType.Asset
        };

        var mockAccountsSet = new Mock<DbSet<Account>>();
        var accountsList = new List<Account> { existingAccount }.AsQueryable();
        
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Provider).Returns(accountsList.Provider);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Expression).Returns(accountsList.Expression);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.ElementType).Returns(accountsList.ElementType);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.GetEnumerator()).Returns(accountsList.GetEnumerator());

        _contextMock.Setup(c => c.Accounts).Returns(mockAccountsSet.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.That(ex.Message, Does.Contain("Account with code '1000' already exists"));
    }

    [Test]
    public async Task Handle_ThrowsException_WhenParentAccountDoesNotExist()
    {
        // Arrange
        var createAccountDto = new CreateAccountDto
        {
            AccountCode = "1100",
            AccountName = "Checking Account",
            AccountType = AccountType.Asset,
            Description = "Checking account",
            ParentAccountId = 999
        };

        var command = new CreateAccountCommand(createAccountDto);

        var mockAccountsSet = new Mock<DbSet<Account>>();
        var accountsList = new List<Account>().AsQueryable();
        
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Provider).Returns(accountsList.Provider);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Expression).Returns(accountsList.Expression);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.ElementType).Returns(accountsList.ElementType);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.GetEnumerator()).Returns(accountsList.GetEnumerator());

        _contextMock.Setup(c => c.Accounts).Returns(mockAccountsSet.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.That(ex.Message, Does.Contain("Parent account with ID '999' does not exist"));
    }

    [Test]
    public async Task Handle_CreatesAccountWithParent_WhenParentExists()
    {
        // Arrange
        var createAccountDto = new CreateAccountDto
        {
            AccountCode = "1100",
            AccountName = "Checking Account",
            AccountType = AccountType.Asset,
            Description = "Checking account",
            ParentAccountId = 1
        };

        var command = new CreateAccountCommand(createAccountDto);

        var parentAccount = new Account
        {
            Id = 1,
            AccountCode = "1000",
            AccountName = "Cash",
            AccountType = AccountType.Asset
        };

        var accountEntity = new Account
        {
            Id = 2,
            AccountCode = "1100",
            AccountName = "Checking Account",
            AccountType = AccountType.Asset,
            Description = "Checking account",
            ParentAccountId = 1
        };

        var expectedDto = new AccountDto
        {
            Id = 2,
            AccountCode = "1100",
            AccountName = "Checking Account",
            AccountType = AccountType.Asset,
            Description = "Checking account",
            ParentAccountId = 1
        };

        var mockAccountsSet = new Mock<DbSet<Account>>();
        var accountsList = new List<Account> { parentAccount }.AsQueryable();
        
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Provider).Returns(accountsList.Provider);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Expression).Returns(accountsList.Expression);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.ElementType).Returns(accountsList.ElementType);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.GetEnumerator()).Returns(accountsList.GetEnumerator());

        _contextMock.Setup(c => c.Accounts).Returns(mockAccountsSet.Object);
        _mapperMock.Setup(m => m.ToEntity(createAccountDto)).Returns(accountEntity);
        _mapperMock.Setup(m => m.ToDto(accountEntity)).Returns(expectedDto);
        _currentUserServiceMock.Setup(s => s.GetCurrentUserForAudit()).Returns("testuser");
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(expectedDto));
        Assert.That(accountEntity.CreatedBy, Is.EqualTo("testuser"));
        Assert.That(accountEntity.UpdatedBy, Is.EqualTo("testuser"));
    }
}