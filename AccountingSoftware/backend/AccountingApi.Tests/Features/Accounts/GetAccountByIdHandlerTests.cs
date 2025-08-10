using AccountingApi.DTOs;
using AccountingApi.Features.Accounts;
using AccountingApi.Models;
using AccountingApi.Tests.TestHelpers;

namespace AccountingApi.Tests.Features.Accounts;

public class GetAccountByIdHandlerTests : BaseTestWithInMemoryDb
{
    private GetAccountByIdQueryHandler _handler = null!;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        
        _handler = new GetAccountByIdQueryHandler(
            Context,
            AccountMapper);
    }

    protected override void SeedTestData()
    {
        AddTestAccounts();
    }

    [Test]
    public async Task Handle_ReturnsAccount_WhenAccountExists()
    {
        // Arrange
        const int accountId = 1; // Use existing test account
        var query = new GetAccountByIdQuery(accountId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(accountId));
        Assert.That(result.AccountCode, Is.EqualTo("1000"));
        Assert.That(result.AccountName, Is.EqualTo("Cash"));
        Assert.That(result.AccountType, Is.EqualTo(AccountType.Asset));
    }

    [Test]
    public async Task Handle_ReturnsNull_WhenAccountDoesNotExist()
    {
        // Arrange
        const int accountId = 999; // Account that doesn't exist
        var query = new GetAccountByIdQuery(accountId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Handle_ReturnsAccountWithoutParent_WhenParentAccountIsNull()
    {
        // Arrange
        var query = new GetAccountByIdQuery(1); // Use existing test account which doesn't have a parent

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ParentAccountId, Is.Null);
        Assert.That(result.ParentAccountName, Is.Null);
    }

    [Test]
    public async Task Handle_LoadsAccountSuccessfully_WithParentAccountInclude()
    {
        // Arrange
        // Add a parent and child account to test Include functionality
        var parentAccount = new Account
        {
            Id = 10,
            AccountCode = "1000",
            AccountName = "Assets",
            AccountType = AccountType.Asset,
            IsActive = true,
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = "test-user",
            UpdatedAt = DateTime.UtcNow
        };

        var childAccount = new Account
        {
            Id = 11,
            AccountCode = "1100",
            AccountName = "Current Assets",
            AccountType = AccountType.Asset,
            ParentAccountId = 10,
            IsActive = true,
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = "test-user",
            UpdatedAt = DateTime.UtcNow
        };

        Context.Accounts.AddRange(parentAccount, childAccount);
        Context.SaveChanges();

        var query = new GetAccountByIdQuery(childAccount.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(childAccount.Id));
        Assert.That(result.ParentAccountId, Is.EqualTo(parentAccount.Id));
        Assert.That(result.ParentAccountName, Is.EqualTo(parentAccount.AccountName));
    }
}