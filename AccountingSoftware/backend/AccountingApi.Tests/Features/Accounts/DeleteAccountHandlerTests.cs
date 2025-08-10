using AccountingApi.Features.Accounts;
using AccountingApi.Models;
using AccountingApi.Tests.TestHelpers;

namespace AccountingApi.Tests.Features.Accounts;

public class DeleteAccountHandlerTests : BaseTestWithInMemoryDb
{
    private DeleteAccountCommandHandler _handler = null!;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        
        _handler = new DeleteAccountCommandHandler(
            Context,
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
        var command = new DeleteAccountCommand(accountId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void Handle_ThrowsException_WhenAccountHasActiveSubAccounts()
    {
        // Arrange
        var parentAccount = new Account
        {
            Id = 4, // Use ID not in test data
            AccountCode = "2000",
            AccountName = "Liabilities",
            AccountType = AccountType.Liability,
            Balance = 0,
            IsActive = true,
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = "test-user",
            UpdatedAt = DateTime.UtcNow
        };

        var subAccount = new Account
        {
            Id = 5,
            AccountCode = "2100",
            AccountName = "Accounts Payable",
            AccountType = AccountType.Liability,
            ParentAccountId = parentAccount.Id,
            IsActive = true,
            IsDeleted = false,
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = "test-user",
            UpdatedAt = DateTime.UtcNow
        };

        Context.Accounts.AddRange(parentAccount, subAccount);
        Context.SaveChanges();

        var command = new DeleteAccountCommand(parentAccount.Id);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public void Handle_ThrowsException_WhenAccountHasActiveJournalEntryLines()
    {
        // Arrange
        const int accountId = 1; // Use existing test account
        
        // Add a journal entry line to the existing account
        var journalEntry = new JournalEntry
        {
            Id = 1,
            Description = "Test Entry",
            TransactionDate = DateTime.UtcNow,
            Reference = "TEST001",
            IsPosted = false,
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = "test-user",
            UpdatedAt = DateTime.UtcNow
        };

        var journalEntryLine = new JournalEntryLine
        {
            Id = 1,
            JournalEntryId = journalEntry.Id,
            AccountId = accountId,
            DebitAmount = 100,
            CreditAmount = 0,
            Description = "Test line",
            IsDeleted = false,
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = "test-user",
            UpdatedAt = DateTime.UtcNow
        };

        Context.JournalEntries.Add(journalEntry);
        Context.JournalEntryLines.Add(journalEntryLine);
        Context.SaveChanges();

        var command = new DeleteAccountCommand(accountId);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public void Handle_ThrowsException_WhenAccountHasNonZeroBalance()
    {
        // Arrange
        const int accountId = 1; // Use existing test account
        
        // Update the account balance to non-zero
        var account = Context.Accounts.Find(accountId);
        account!.Balance = 1000;
        Context.SaveChanges();

        var command = new DeleteAccountCommand(accountId);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Handle_PerformsSoftDelete_WhenAccountCanBeDeleted()
    {
        // Arrange
        const int accountId = 2; // Use existing test account that won't have journal entries

        var command = new DeleteAccountCommand(accountId);

        // Get the account before deletion
        var accountBefore = Context.Accounts.Find(accountId);
        Assert.That(accountBefore, Is.Not.Null);
        Assert.That(accountBefore.IsDeleted, Is.False);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        
        // Reload the account to check changes
        Context.Entry(accountBefore).Reload();
        Assert.That(accountBefore.IsDeleted, Is.True);
        Assert.That(accountBefore.DeletedBy, Is.EqualTo("test-user-id"));
        Assert.That(accountBefore.UpdatedBy, Is.EqualTo("test-user-id"));
        Assert.That(accountBefore.DeletedAt, Is.Not.Null);
        Assert.That(accountBefore.UpdatedAt, Is.Not.Null);
    }

    [Test]
    public async Task Handle_AllowsDeletion_WhenSubAccountsAreDeleted()
    {
        // Arrange
        var parentAccount = new Account
        {
            Id = 6,
            AccountCode = "3000",
            AccountName = "Income",
            AccountType = AccountType.Revenue,
            Balance = 0,
            IsActive = true,
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = "test-user",
            UpdatedAt = DateTime.UtcNow
        };

        var deletedSubAccount = new Account
        {
            Id = 7,
            AccountCode = "3100",
            AccountName = "Sales Income",
            AccountType = AccountType.Revenue,
            ParentAccountId = parentAccount.Id,
            IsActive = true,
            IsDeleted = true, // Already deleted
            DeletedBy = "test-user",
            DeletedAt = DateTime.UtcNow,
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = "test-user",
            UpdatedAt = DateTime.UtcNow
        };

        Context.Accounts.AddRange(parentAccount, deletedSubAccount);
        Context.SaveChanges();

        var command = new DeleteAccountCommand(parentAccount.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        
        // Reload and check the parent account is deleted
        Context.Entry(parentAccount).Reload();
        Assert.That(parentAccount.IsDeleted, Is.True);
    }

    [Test]
    public async Task Handle_AllowsDeletion_WhenJournalEntryLinesAreDeleted()
    {
        // Arrange
        const int accountId = 3; // Use existing test account
        
        // Add a deleted journal entry line to the account
        var journalEntry = new JournalEntry
        {
            Id = 2,
            Description = "Test Entry for Deleted Line",
            TransactionDate = DateTime.UtcNow,
            Reference = "TEST002",
            IsPosted = false,
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = "test-user",
            UpdatedAt = DateTime.UtcNow
        };

        var deletedJournalEntryLine = new JournalEntryLine
        {
            Id = 2,
            JournalEntryId = journalEntry.Id,
            AccountId = accountId,
            DebitAmount = 0,
            CreditAmount = 200,
            Description = "Test deleted line",
            IsDeleted = true, // Already deleted
            DeletedBy = "test-user",
            DeletedAt = DateTime.UtcNow,
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = "test-user",
            UpdatedAt = DateTime.UtcNow
        };

        Context.JournalEntries.Add(journalEntry);
        Context.JournalEntryLines.Add(deletedJournalEntryLine);
        Context.SaveChanges();

        var command = new DeleteAccountCommand(accountId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        
        // Reload and check the account is deleted
        var account = Context.Accounts.Find(accountId);
        Context.Entry(account!).Reload();
        Assert.That(account.IsDeleted, Is.True);
    }
}