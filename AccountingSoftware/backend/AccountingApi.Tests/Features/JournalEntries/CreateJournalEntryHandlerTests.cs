using AccountingApi.DTOs;
using AccountingApi.Features.JournalEntries;
using AccountingApi.Models;
using AccountingApi.Tests.TestHelpers;

namespace AccountingApi.Tests.Features.JournalEntries;

public class CreateJournalEntryHandlerTests : BaseTestWithInMemoryDb
{
    private CreateJournalEntryCommandHandler _handler = null!;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        
        _handler = new CreateJournalEntryCommandHandler(
            Context,
            JournalEntryMapper,
            CurrentUserServiceMock.Object);
    }

    protected override void SeedTestData()
    {
        AddTestAccounts();
    }

    [Test]
    public async Task Handle_CreatesJournalEntry_WhenValidBalancedEntry()
    {
        // Arrange
        var createJournalEntryDto = new CreateJournalEntryDto
        {
            TransactionDate = DateTime.UtcNow.Date,
            Description = "Test Journal Entry",
            Reference = "REF-001",
            Lines = new List<CreateJournalEntryLineDto>
            {
                new() { AccountId = 1, DebitAmount = 1000, CreditAmount = 0, Description = "Debit line" },
                new() { AccountId = 2, DebitAmount = 0, CreditAmount = 1000, Description = "Credit line" }
            }
        };

        var command = new CreateJournalEntryCommand(createJournalEntryDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Description, Is.EqualTo("Test Journal Entry"));
        Assert.That(result.Reference, Is.EqualTo("REF-001"));
        
        // Verify the journal entry was created in the database
        var createdEntry = Context.JournalEntries.FirstOrDefault(j => j.Description == "Test Journal Entry");
        Assert.That(createdEntry, Is.Not.Null);
        Assert.That(createdEntry.Lines.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task Handle_ThrowsException_WhenJournalEntryIsNotBalanced()
    {
        // Arrange
        var createJournalEntryDto = new CreateJournalEntryDto
        {
            TransactionDate = DateTime.UtcNow.Date,
            Description = "Unbalanced Journal Entry",
            Lines = new List<CreateJournalEntryLineDto>
            {
                new() { AccountId = 1, DebitAmount = 1000, CreditAmount = 0, Description = "Debit line" },
                new() { AccountId = 2, DebitAmount = 0, CreditAmount = 500, Description = "Credit line" } // Unbalanced
            }
        };

        var command = new CreateJournalEntryCommand(createJournalEntryDto);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

    }

    [Test]
    public async Task Handle_ThrowsException_WhenLineHasBothDebitAndCredit()
    {
        // Arrange
        var createJournalEntryDto = new CreateJournalEntryDto
        {
            TransactionDate = DateTime.UtcNow.Date,
            Description = "Invalid Journal Entry",
            Lines = new List<CreateJournalEntryLineDto>
            {
                new() { AccountId = 1, DebitAmount = 1000, CreditAmount = 500, Description = "Invalid line" }, // Both debit and credit
                new() { AccountId = 2, DebitAmount = 0, CreditAmount = 500, Description = "Credit line" }
            }
        };

        var command = new CreateJournalEntryCommand(createJournalEntryDto);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

    }

    [Test]
    public async Task Handle_ThrowsException_WhenLineHasNeitherDebitNorCredit()
    {
        // Arrange
        var createJournalEntryDto = new CreateJournalEntryDto
        {
            TransactionDate = DateTime.UtcNow.Date,
            Description = "Invalid Journal Entry",
            Lines = new List<CreateJournalEntryLineDto>
            {
                new() { AccountId = 1, DebitAmount = 0, CreditAmount = 0, Description = "Invalid line" }, // Neither debit nor credit
                new() { AccountId = 2, DebitAmount = 0, CreditAmount = 1000, Description = "Credit line" }
            }
        };

        var command = new CreateJournalEntryCommand(createJournalEntryDto);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

    }

    [Test]
    public async Task Handle_ThrowsException_WhenReferencedAccountDoesNotExist()
    {
        // Arrange
        var createJournalEntryDto = new CreateJournalEntryDto
        {
            TransactionDate = DateTime.UtcNow.Date,
            Description = "Journal Entry with Missing Account",
            Lines = new List<CreateJournalEntryLineDto>
            {
                new() { AccountId = 1, DebitAmount = 1000, CreditAmount = 0, Description = "Debit line" },
                new() { AccountId = 999, DebitAmount = 0, CreditAmount = 1000, Description = "Credit line" } // Account doesn't exist
            }
        };

        var command = new CreateJournalEntryCommand(createJournalEntryDto);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Handle_ThrowsException_WhenReferencedAccountIsDeleted()
    {
        // Arrange
        var createJournalEntryDto = new CreateJournalEntryDto
        {
            TransactionDate = DateTime.UtcNow.Date,
            Description = "Journal Entry with Deleted Account",
            Lines = new List<CreateJournalEntryLineDto>
            {
                new() { AccountId = 1, DebitAmount = 1000, CreditAmount = 0, Description = "Debit line" },
                new() { AccountId = 2, DebitAmount = 0, CreditAmount = 1000, Description = "Credit line" }
            }
        };

        var command = new CreateJournalEntryCommand(createJournalEntryDto);

        // Soft delete account 2
        var account2 = Context.Accounts.Find(2);
        account2!.IsDeleted = true;
        Context.SaveChanges();

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Handle_AcceptsBalancedEntry_WithSmallRoundingDifference()
    {
        // Arrange
        var createJournalEntryDto = new CreateJournalEntryDto
        {
            TransactionDate = DateTime.UtcNow.Date,
            Description = "Journal Entry with Small Rounding",
            Lines = new List<CreateJournalEntryLineDto>
            {
                new() { AccountId = 1, DebitAmount = 100.005m, CreditAmount = 0, Description = "Debit line" },
                new() { AccountId = 2, DebitAmount = 0, CreditAmount = 100.00m, Description = "Credit line" }
            }
        };

        var command = new CreateJournalEntryCommand(createJournalEntryDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Description, Is.EqualTo("Journal Entry with Small Rounding"));
        
        // Verify the journal entry was created in the database
        var createdEntry = Context.JournalEntries.FirstOrDefault(j => j.Description == "Journal Entry with Small Rounding");
        Assert.That(createdEntry, Is.Not.Null);
        Assert.That(createdEntry.Lines.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task Handle_SetsAuditFields_WhenCreatingJournalEntry()
    {
        // Arrange
        var createJournalEntryDto = new CreateJournalEntryDto
        {
            TransactionDate = DateTime.UtcNow.Date,
            Description = "Audit Test Journal Entry",
            Lines = new List<CreateJournalEntryLineDto>
            {
                new() { AccountId = 1, DebitAmount = 500, CreditAmount = 0, Description = "Debit line" },
                new() { AccountId = 2, DebitAmount = 0, CreditAmount = 500, Description = "Credit line" }
            }
        };

        var command = new CreateJournalEntryCommand(createJournalEntryDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Description, Is.EqualTo("Audit Test Journal Entry"));
        
        // Verify the journal entry was created in the database
        var createdEntry = Context.JournalEntries.FirstOrDefault(j => j.Description == "Audit Test Journal Entry");
        Assert.That(createdEntry, Is.Not.Null);
        Assert.That(createdEntry.CreatedBy, Is.EqualTo("test-user-id"));
        Assert.That(createdEntry.UpdatedBy, Is.EqualTo("test-user-id"));
        Assert.That(createdEntry.CreatedAt, Is.Not.Null);
        Assert.That(createdEntry.UpdatedAt, Is.Not.Null);
    }
}