using AccountingApi.DTOs;
using AccountingApi.Features.Accounts;
using AccountingApi.Models;
using AccountingApi.Tests.TestHelpers;

namespace AccountingApi.Tests.Features.Accounts;

public class UpdateAccountHandlerTests : BaseTestWithInMemoryDb
{
    private UpdateAccountCommandHandler _handler = null!;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        
        _handler = new UpdateAccountCommandHandler(
            Context,
            AccountMapper,
            CurrentUserServiceMock.Object);
    }

    protected override void SeedTestData()
    {
        AddTestAccounts();
    }

    [Test]
    public async Task Handle_ReturnsFalse_WhenAccountNotFound()
    {
        // Arrange
        const int accountId = 999; // Account that doesn't exist
        var updateAccountDto = new UpdateAccountDto
        {
            AccountName = "Updated Name",
            Description = "Updated description",
            IsActive = true
        };
        var command = new UpdateAccountCommand(accountId, updateAccountDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task Handle_UpdatesAccountSuccessfully_WhenAccountExists()
    {
        // Arrange
        const int accountId = 1; // Use existing test account
        var updateAccountDto = new UpdateAccountDto
        {
            AccountName = "Updated Cash Account",
            Description = "Updated cash account description",
            IsActive = false
        };
        var command = new UpdateAccountCommand(accountId, updateAccountDto);

        // Get the account before update
        var accountBefore = Context.Accounts.Find(accountId);
        Assert.That(accountBefore, Is.Not.Null);
        var originalName = accountBefore.AccountName;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        
        // Reload the account to check changes
        Context.Entry(accountBefore).Reload();
        Assert.That(accountBefore.AccountName, Is.EqualTo("Updated Cash Account"));
        Assert.That(accountBefore.Description, Is.EqualTo("Updated cash account description"));
        Assert.That(accountBefore.IsActive, Is.False);
        Assert.That(accountBefore.UpdatedBy, Is.EqualTo("test-user-id"));
        Assert.That(accountBefore.UpdatedAt, Is.Not.Null);
    }

    [Test]
    public async Task Handle_SetsAuditInformation_WhenUpdatingAccount()
    {
        // Arrange
        const int accountId = 2; // Use a different test account
        var updateAccountDto = new UpdateAccountDto
        {
            AccountName = "Updated Name",
            Description = "Updated description",
            IsActive = true
        };
        var command = new UpdateAccountCommand(accountId, updateAccountDto);

        var beforeUpdate = DateTime.UtcNow;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        
        // Reload the account to check audit information
        var updatedAccount = Context.Accounts.Find(accountId);
        Context.Entry(updatedAccount!).Reload();
        Assert.That(updatedAccount.UpdatedBy, Is.EqualTo("test-user-id"));
        Assert.That(updatedAccount.UpdatedAt, Is.GreaterThanOrEqualTo(beforeUpdate));
        Assert.That(updatedAccount.UpdatedAt, Is.LessThanOrEqualTo(DateTime.UtcNow));
    }

    [Test]
    public async Task Handle_UpdatesEntityFields_WithCorrectValues()
    {
        // Arrange
        const int accountId = 3; // Use another test account
        var updateAccountDto = new UpdateAccountDto
        {
            AccountName = "Test Account Updated",
            Description = "Test description updated",
            IsActive = false
        };
        var command = new UpdateAccountCommand(accountId, updateAccountDto);

        // Get original values
        var originalAccount = Context.Accounts.Find(accountId);
        var originalName = originalAccount!.AccountName;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        
        // Verify the account was updated with correct values
        Context.Entry(originalAccount).Reload();
        Assert.That(originalAccount.AccountName, Is.EqualTo("Test Account Updated"));
        Assert.That(originalAccount.Description, Is.EqualTo("Test description updated"));
        Assert.That(originalAccount.IsActive, Is.False);
        Assert.That(originalAccount.AccountName, Is.Not.EqualTo(originalName));
    }
}