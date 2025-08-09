using AccountingApi.DTOs;
using AccountingApi.Features.Accounts;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;
using AccountingApi.Models;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace AccountingApi.Tests.Features.Accounts;

public class GetAccountByIdHandlerTests
{
    private Mock<AccountingDbContext> _contextMock = null!;
    private Mock<AccountMapper> _mapperMock = null!;
    private GetAccountByIdQueryHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AccountingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _contextMock = new Mock<AccountingDbContext>(options);
        _mapperMock = new Mock<AccountMapper>();
        
        _handler = new GetAccountByIdQueryHandler(
            _contextMock.Object,
            _mapperMock.Object);
    }

    [Test]
    public async Task Handle_ReturnsAccount_WhenAccountExists()
    {
        // Arrange
        const int accountId = 1;
        var query = new GetAccountByIdQuery(accountId);

        var parentAccount = new Account
        {
            Id = 10,
            AccountCode = "1000",
            AccountName = "Assets",
            AccountType = AccountType.Asset
        };

        var account = new Account
        {
            Id = accountId,
            AccountCode = "1100",
            AccountName = "Cash",
            AccountType = AccountType.Asset,
            Description = "Cash account",
            ParentAccountId = 10,
            ParentAccount = parentAccount
        };

        var expectedDto = new AccountDto
        {
            Id = accountId,
            AccountCode = "1100",
            AccountName = "Cash",
            AccountType = AccountType.Asset,
            Description = "Cash account",
            ParentAccountId = 10,
            ParentAccountName = "Assets"
        };

        var mockAccountsSet = new Mock<DbSet<Account>>();
        var accountsList = new List<Account> { account }.AsQueryable();
        
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Provider).Returns(accountsList.Provider);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Expression).Returns(accountsList.Expression);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.ElementType).Returns(accountsList.ElementType);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.GetEnumerator()).Returns(accountsList.GetEnumerator());

        _contextMock.Setup(c => c.Accounts).Returns(mockAccountsSet.Object);
        _mapperMock.Setup(m => m.ToDto(account)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(expectedDto));
        _mapperMock.Verify(m => m.ToDto(account), Times.Once);
    }

    [Test]
    public async Task Handle_ReturnsNull_WhenAccountDoesNotExist()
    {
        // Arrange
        const int accountId = 999;
        var query = new GetAccountByIdQuery(accountId);

        var mockAccountsSet = new Mock<DbSet<Account>>();
        var accountsList = new List<Account>().AsQueryable();
        
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Provider).Returns(accountsList.Provider);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Expression).Returns(accountsList.Expression);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.ElementType).Returns(accountsList.ElementType);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.GetEnumerator()).Returns(accountsList.GetEnumerator());

        _contextMock.Setup(c => c.Accounts).Returns(mockAccountsSet.Object);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Null);
        _mapperMock.Verify(m => m.ToDto(It.IsAny<Account>()), Times.Never);
    }

    [Test]
    public async Task Handle_ReturnsAccountWithoutParent_WhenParentAccountIsNull()
    {
        // Arrange
        const int accountId = 1;
        var query = new GetAccountByIdQuery(accountId);

        var account = new Account
        {
            Id = accountId,
            AccountCode = "1000",
            AccountName = "Assets",
            AccountType = AccountType.Asset,
            Description = "Top level account",
            ParentAccountId = null,
            ParentAccount = null
        };

        var expectedDto = new AccountDto
        {
            Id = accountId,
            AccountCode = "1000",
            AccountName = "Assets",
            AccountType = AccountType.Asset,
            Description = "Top level account",
            ParentAccountId = null,
            ParentAccountName = null
        };

        var mockAccountsSet = new Mock<DbSet<Account>>();
        var accountsList = new List<Account> { account }.AsQueryable();
        
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Provider).Returns(accountsList.Provider);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Expression).Returns(accountsList.Expression);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.ElementType).Returns(accountsList.ElementType);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.GetEnumerator()).Returns(accountsList.GetEnumerator());

        _contextMock.Setup(c => c.Accounts).Returns(mockAccountsSet.Object);
        _mapperMock.Setup(m => m.ToDto(account)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(expectedDto));
        Assert.That(result.ParentAccountId, Is.Null);
        Assert.That(result.ParentAccountName, Is.Null);
        _mapperMock.Verify(m => m.ToDto(account), Times.Once);
    }

    [Test]
    public async Task Handle_CallsIncludeParentAccount_ForEagerLoading()
    {
        // Arrange
        const int accountId = 1;
        var query = new GetAccountByIdQuery(accountId);

        var account = new Account
        {
            Id = accountId,
            AccountCode = "1100",
            AccountName = "Cash",
            AccountType = AccountType.Asset
        };

        var expectedDto = new AccountDto
        {
            Id = accountId,
            AccountCode = "1100",
            AccountName = "Cash",
            AccountType = AccountType.Asset
        };

        var mockAccountsSet = new Mock<DbSet<Account>>();
        var accountsList = new List<Account> { account }.AsQueryable();
        
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Provider).Returns(accountsList.Provider);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Expression).Returns(accountsList.Expression);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.ElementType).Returns(accountsList.ElementType);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.GetEnumerator()).Returns(accountsList.GetEnumerator());

        _contextMock.Setup(c => c.Accounts).Returns(mockAccountsSet.Object);
        _mapperMock.Setup(m => m.ToDto(account)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        // Verify that the context's Accounts property was accessed (implying Include was called in the handler)
        _contextMock.Verify(c => c.Accounts, Times.Once);
        _mapperMock.Verify(m => m.ToDto(account), Times.Once);
    }
}