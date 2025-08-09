using AccountingApi.DTOs;
using AccountingApi.Features.Accounts;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;
using AccountingApi.Models;
using AccountingApi.Services.CurrentUserService;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace AccountingApi.Tests.Features.Accounts;

public class UpdateAccountHandlerTests
{
    private Mock<AccountingDbContext> _contextMock = null!;
    private Mock<AccountMapper> _mapperMock = null!;
    private Mock<ICurrentUserService> _currentUserServiceMock = null!;
    private UpdateAccountCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AccountingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _contextMock = new Mock<AccountingDbContext>(options);
        _mapperMock = new Mock<AccountMapper>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        
        _handler = new UpdateAccountCommandHandler(
            _contextMock.Object,
            _mapperMock.Object,
            _currentUserServiceMock.Object);
    }

    [Test]
    public async Task Handle_ReturnsFalse_WhenAccountNotFound()
    {
        // Arrange
        const int accountId = 999;
        var updateAccountDto = new UpdateAccountDto
        {
            AccountName = "Updated Name",
            Description = "Updated description",
            IsActive = true
        };
        var command = new UpdateAccountCommand(accountId, updateAccountDto);

        var mockAccountsSet = new Mock<DbSet<Account>>();
        var accountsList = new List<Account>().AsQueryable();
        
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Provider).Returns(accountsList.Provider);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Expression).Returns(accountsList.Expression);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.ElementType).Returns(accountsList.ElementType);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.GetEnumerator()).Returns(accountsList.GetEnumerator());

        _contextMock.Setup(c => c.Accounts).Returns(mockAccountsSet.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.False);
        _mapperMock.Verify(m => m.UpdateEntity(It.IsAny<Account>(), It.IsAny<UpdateAccountDto>()), Times.Never);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_UpdatesAccountSuccessfully_WhenAccountExists()
    {
        // Arrange
        const int accountId = 1;
        var updateAccountDto = new UpdateAccountDto
        {
            AccountName = "Updated Cash Account",
            Description = "Updated cash account description",
            IsActive = false
        };
        var command = new UpdateAccountCommand(accountId, updateAccountDto);

        var account = new Account
        {
            Id = accountId,
            AccountCode = "1000",
            AccountName = "Cash",
            AccountType = AccountType.Asset,
            Description = "Original description",
            IsActive = true
        };

        var mockAccountsSet = new Mock<DbSet<Account>>();
        var accountsList = new List<Account> { account }.AsQueryable();
        
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Provider).Returns(accountsList.Provider);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Expression).Returns(accountsList.Expression);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.ElementType).Returns(accountsList.ElementType);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.GetEnumerator()).Returns(accountsList.GetEnumerator());

        _contextMock.Setup(c => c.Accounts).Returns(mockAccountsSet.Object);
        _currentUserServiceMock.Setup(s => s.GetCurrentUserForAudit()).Returns("testuser");
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(account.UpdatedBy, Is.EqualTo("testuser"));
        Assert.That(account.UpdatedAt, Is.Not.Null);
        
        _mapperMock.Verify(m => m.UpdateEntity(account, updateAccountDto), Times.Once);
        _currentUserServiceMock.Verify(s => s.GetCurrentUserForAudit(), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_SetsAuditInformation_WhenUpdatingAccount()
    {
        // Arrange
        const int accountId = 1;
        var updateAccountDto = new UpdateAccountDto
        {
            AccountName = "Updated Name",
            Description = "Updated description",
            IsActive = true
        };
        var command = new UpdateAccountCommand(accountId, updateAccountDto);

        var account = new Account
        {
            Id = accountId,
            AccountCode = "1000",
            AccountName = "Original Name",
            AccountType = AccountType.Asset,
            UpdatedBy = "olduser",
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var mockAccountsSet = new Mock<DbSet<Account>>();
        var accountsList = new List<Account> { account }.AsQueryable();
        
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Provider).Returns(accountsList.Provider);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Expression).Returns(accountsList.Expression);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.ElementType).Returns(accountsList.ElementType);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.GetEnumerator()).Returns(accountsList.GetEnumerator());

        _contextMock.Setup(c => c.Accounts).Returns(mockAccountsSet.Object);
        _currentUserServiceMock.Setup(s => s.GetCurrentUserForAudit()).Returns("newuser");
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var beforeUpdate = DateTime.UtcNow;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(account.UpdatedBy, Is.EqualTo("newuser"));
        Assert.That(account.UpdatedAt, Is.GreaterThanOrEqualTo(beforeUpdate));
        Assert.That(account.UpdatedAt, Is.LessThanOrEqualTo(DateTime.UtcNow));
    }

    [Test]
    public async Task Handle_CallsMapperUpdateEntity_WithCorrectParameters()
    {
        // Arrange
        const int accountId = 1;
        var updateAccountDto = new UpdateAccountDto
        {
            AccountName = "Test Account",
            Description = "Test description",
            IsActive = true
        };
        var command = new UpdateAccountCommand(accountId, updateAccountDto);

        var account = new Account
        {
            Id = accountId,
            AccountCode = "1000",
            AccountName = "Original Name",
            AccountType = AccountType.Asset
        };

        var mockAccountsSet = new Mock<DbSet<Account>>();
        var accountsList = new List<Account> { account }.AsQueryable();
        
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Provider).Returns(accountsList.Provider);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Expression).Returns(accountsList.Expression);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.ElementType).Returns(accountsList.ElementType);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.GetEnumerator()).Returns(accountsList.GetEnumerator());

        _contextMock.Setup(c => c.Accounts).Returns(mockAccountsSet.Object);
        _currentUserServiceMock.Setup(s => s.GetCurrentUserForAudit()).Returns("testuser");
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        _mapperMock.Verify(m => m.UpdateEntity(
            It.Is<Account>(a => a.Id == accountId),
            It.Is<UpdateAccountDto>(dto => 
                dto.AccountName == updateAccountDto.AccountName &&
                dto.Description == updateAccountDto.Description &&
                dto.IsActive == updateAccountDto.IsActive)), 
            Times.Once);
    }
}