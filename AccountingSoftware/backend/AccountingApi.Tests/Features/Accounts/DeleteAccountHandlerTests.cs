using AccountingApi.Features.Accounts;
using AccountingApi.Infrastructure;
using AccountingApi.Models;
using AccountingApi.Services.CurrentUserService;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace AccountingApi.Tests.Features.Accounts;

public class DeleteAccountHandlerTests
{
    private Mock<AccountingDbContext> _contextMock = null!;
    private Mock<ICurrentUserService> _currentUserServiceMock = null!;
    private DeleteAccountCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AccountingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _contextMock = new Mock<AccountingDbContext>(options);
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        
        _handler = new DeleteAccountCommandHandler(
            _contextMock.Object,
            _currentUserServiceMock.Object);
    }

    [Test]
    public async Task Handle_ReturnsFalse_WhenAccountNotFound()
    {
        // Arrange
        const int accountId = 999;
        var command = new DeleteAccountCommand(accountId);

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
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_ThrowsException_WhenAccountHasActiveSubAccounts()
    {
        // Arrange
        const int accountId = 1;
        var command = new DeleteAccountCommand(accountId);

        var account = new Account
        {
            Id = accountId,
            AccountCode = "1000",
            AccountName = "Assets",
            AccountType = AccountType.Asset,
            Balance = 0,
            SubAccounts = new List<Account>
            {
                new() { Id = 2, AccountCode = "1100", AccountName = "Cash", IsDeleted = false }
            },
            JournalEntryLines = new List<JournalEntryLine>()
        };

        var mockAccountsSet = new Mock<DbSet<Account>>();
        var accountsList = new List<Account> { account }.AsQueryable();
        
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Provider).Returns(accountsList.Provider);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Expression).Returns(accountsList.Expression);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.ElementType).Returns(accountsList.ElementType);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.GetEnumerator()).Returns(accountsList.GetEnumerator());

        _contextMock.Setup(c => c.Accounts).Returns(mockAccountsSet.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.That(ex.Message, Does.Contain("Cannot delete account that has active sub-accounts"));
    }

    [Test]
    public async Task Handle_ThrowsException_WhenAccountHasActiveJournalEntryLines()
    {
        // Arrange
        const int accountId = 1;
        var command = new DeleteAccountCommand(accountId);

        var account = new Account
        {
            Id = accountId,
            AccountCode = "1000",
            AccountName = "Cash",
            AccountType = AccountType.Asset,
            Balance = 0,
            SubAccounts = new List<Account>(),
            JournalEntryLines = new List<JournalEntryLine>
            {
                new() { Id = 1, AccountId = accountId, IsDeleted = false }
            }
        };

        var mockAccountsSet = new Mock<DbSet<Account>>();
        var accountsList = new List<Account> { account }.AsQueryable();
        
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Provider).Returns(accountsList.Provider);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Expression).Returns(accountsList.Expression);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.ElementType).Returns(accountsList.ElementType);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.GetEnumerator()).Returns(accountsList.GetEnumerator());

        _contextMock.Setup(c => c.Accounts).Returns(mockAccountsSet.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.That(ex.Message, Does.Contain("Cannot delete account that has active journal entry lines"));
    }

    [Test]
    public async Task Handle_ThrowsException_WhenAccountHasNonZeroBalance()
    {
        // Arrange
        const int accountId = 1;
        var command = new DeleteAccountCommand(accountId);

        var account = new Account
        {
            Id = accountId,
            AccountCode = "1000",
            AccountName = "Cash",
            AccountType = AccountType.Asset,
            Balance = 1000,
            SubAccounts = new List<Account>(),
            JournalEntryLines = new List<JournalEntryLine>()
        };

        var mockAccountsSet = new Mock<DbSet<Account>>();
        var accountsList = new List<Account> { account }.AsQueryable();
        
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Provider).Returns(accountsList.Provider);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.Expression).Returns(accountsList.Expression);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.ElementType).Returns(accountsList.ElementType);
        mockAccountsSet.As<IQueryable<Account>>().Setup(m => m.GetEnumerator()).Returns(accountsList.GetEnumerator());

        _contextMock.Setup(c => c.Accounts).Returns(mockAccountsSet.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.That(ex.Message, Does.Contain("Cannot delete account with non-zero balance"));
    }

    [Test]
    public async Task Handle_PerformsSoftDelete_WhenAccountCanBeDeleted()
    {
        // Arrange
        const int accountId = 1;
        var command = new DeleteAccountCommand(accountId);

        var account = new Account
        {
            Id = accountId,
            AccountCode = "1000",
            AccountName = "Cash",
            AccountType = AccountType.Asset,
            Balance = 0,
            IsDeleted = false,
            SubAccounts = new List<Account>(),
            JournalEntryLines = new List<JournalEntryLine>()
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
        Assert.That(account.IsDeleted, Is.True);
        Assert.That(account.DeletedBy, Is.EqualTo("testuser"));
        Assert.That(account.UpdatedBy, Is.EqualTo("testuser"));
        Assert.That(account.DeletedAt, Is.Not.Null);
        Assert.That(account.UpdatedAt, Is.Not.Null);
        
        _currentUserServiceMock.Verify(s => s.GetCurrentUserForAudit(), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_AllowsDeletion_WhenSubAccountsAreDeleted()
    {
        // Arrange
        const int accountId = 1;
        var command = new DeleteAccountCommand(accountId);

        var account = new Account
        {
            Id = accountId,
            AccountCode = "1000",
            AccountName = "Assets",
            AccountType = AccountType.Asset,
            Balance = 0,
            SubAccounts = new List<Account>
            {
                new() { Id = 2, AccountCode = "1100", AccountName = "Cash", IsDeleted = true }
            },
            JournalEntryLines = new List<JournalEntryLine>()
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
        Assert.That(account.IsDeleted, Is.True);
    }

    [Test]
    public async Task Handle_AllowsDeletion_WhenJournalEntryLinesAreDeleted()
    {
        // Arrange
        const int accountId = 1;
        var command = new DeleteAccountCommand(accountId);

        var account = new Account
        {
            Id = accountId,
            AccountCode = "1000",
            AccountName = "Cash",
            AccountType = AccountType.Asset,
            Balance = 0,
            SubAccounts = new List<Account>(),
            JournalEntryLines = new List<JournalEntryLine>
            {
                new() { Id = 1, AccountId = accountId, IsDeleted = true }
            }
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
        Assert.That(account.IsDeleted, Is.True);
    }
}