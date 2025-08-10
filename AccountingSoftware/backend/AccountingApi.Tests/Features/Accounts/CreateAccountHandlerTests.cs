using AccountingApi.DTOs;
using AccountingApi.Features.Accounts;
using AccountingApi.Models;
using AccountingApi.Tests.TestHelpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace AccountingApi.Tests.Features.Accounts;

public class CreateAccountHandlerTests : BaseTestWithInMemoryDb
{
    private Mock<ILogger<CreateAccountCommandHandler>> _loggerMock = null!;
    private CreateAccountCommandHandler _handler = null!;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        
        _loggerMock = new Mock<ILogger<CreateAccountCommandHandler>>();
        
        _handler = new CreateAccountCommandHandler(
            Context,
            AccountMapper,
            CurrentUserServiceMock.Object,
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
        CurrentUserServiceMock.Setup(s => s.GetCurrentUserForAudit()).Returns("testuser");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AccountCode, Is.EqualTo("1000"));
        Assert.That(result.AccountName, Is.EqualTo("Cash"));
        Assert.That(result.AccountType, Is.EqualTo(AccountType.Asset));
        Assert.That(result.Description, Is.EqualTo("Cash account"));

        // Verify the account was actually created in the database
        var createdAccount = Context.Accounts.FirstOrDefault(a => a.AccountCode == "1000");
        Assert.That(createdAccount, Is.Not.Null);
        Assert.That(createdAccount!.CreatedBy, Is.EqualTo("testuser"));
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

        // Create an existing account with the same code in the database
        var existingAccount = new Account
        {
            AccountCode = "1000",
            AccountName = "Existing Cash",
            AccountType = AccountType.Asset,
            CreatedBy = "existing-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = "existing-user",
            UpdatedAt = DateTime.UtcNow
        };

        Context.Accounts.Add(existingAccount);
        Context.SaveChanges();

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
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
            ParentAccountId = 999 // Non-existent parent ID
        };

        var command = new CreateAccountCommand(createAccountDto);

        // Don't add any accounts to the database, so parent won't exist

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
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

        // Create a parent account in the database
        var parentAccount = new Account
        {
            Id = 1,
            AccountCode = "1000",
            AccountName = "Cash",
            AccountType = AccountType.Asset,
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = "test-user",
            UpdatedAt = DateTime.UtcNow
        };

        Context.Accounts.Add(parentAccount);
        Context.SaveChanges();

        CurrentUserServiceMock.Setup(s => s.GetCurrentUserForAudit()).Returns("testuser");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AccountCode, Is.EqualTo("1100"));
        Assert.That(result.AccountName, Is.EqualTo("Checking Account"));
        Assert.That(result.ParentAccountId, Is.EqualTo(1));

        // Verify the account was actually created in the database
        var createdAccount = Context.Accounts.FirstOrDefault(a => a.AccountCode == "1100");
        Assert.That(createdAccount, Is.Not.Null);
        Assert.That(createdAccount!.CreatedBy, Is.EqualTo("testuser"));
        Assert.That(createdAccount.UpdatedBy, Is.EqualTo("testuser"));
        Assert.That(createdAccount.ParentAccountId, Is.EqualTo(1));
    }
}