using AccountingApi.DTOs;
using AccountingApi.Features.JournalEntries;
using AccountingApi.Models;
using AccountingApi.Tests.TestHelpers;

namespace AccountingApi.Tests.Features.JournalEntries;

public class UpdateJournalEntryHandlerTests : BaseTestWithInMemoryDb
{
    private UpdateJournalEntryCommandHandler _handler = null!;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        
        _handler = new UpdateJournalEntryCommandHandler(
            Context,
            JournalEntryMapper,
            CurrentUserServiceMock.Object);
    }

    protected override void SeedTestData()
    {
        AddTestAccounts();
    }

    [Test]
    public async Task Handle_UpdatesJournalEntry_WhenValidBalancedUnpostedEntry()
    {
        // Arrange
        var journalEntryId = 1;
        var updateJournalEntryDto = new UpdateJournalEntryDto
        {
            TransactionDate = DateTime.UtcNow.Date,
            Description = "Updated Journal Entry",
            Reference = "REF-001-UPDATED",
            Lines = new List<CreateJournalEntryLineDto>
            {
                new() { AccountId = 1, DebitAmount = 1500, CreditAmount = 0, Description = "Updated debit line" },
                new() { AccountId = 2, DebitAmount = 0, CreditAmount = 1500, Description = "Updated credit line" }
            }
        };

        var command = new UpdateJournalEntryCommand(journalEntryId, updateJournalEntryDto);
        var currentUser = "test-user";

        // Create existing journal entry in database
        var existingJournalEntry = new JournalEntry
        {
            Id = journalEntryId,
            EntryNumber = "JE-001",
            TransactionDate = DateTime.UtcNow.Date.AddDays(-1),
            Description = "Original Journal Entry",
            Reference = "REF-001",
            IsPosted = false,
            CreatedBy = currentUser,
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = currentUser,
            UpdatedAt = DateTime.UtcNow,
            Lines = new List<JournalEntryLine>
            {
                new() { Id = 1, AccountId = 1, DebitAmount = 1000, CreditAmount = 0, Description = "Original debit" },
                new() { Id = 2, AccountId = 2, DebitAmount = 0, CreditAmount = 1000, Description = "Original credit" }
            }
        };

        Context.JournalEntries.Add(existingJournalEntry);
        Context.SaveChanges();

        CurrentUserServiceMock.Setup(x => x.GetCurrentUserForAudit()).Returns(currentUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Description, Is.EqualTo("Updated Journal Entry"));
        Assert.That(result.Reference, Is.EqualTo("REF-001-UPDATED"));
        
        // Verify the entity was actually updated in the database
        var updatedEntry = await Context.JournalEntries.FindAsync(journalEntryId);
        Assert.That(updatedEntry, Is.Not.Null);
        Assert.That(updatedEntry!.UpdatedBy, Is.EqualTo(currentUser));
        Assert.That(updatedEntry.Description, Is.EqualTo("Updated Journal Entry"));
        Assert.That(updatedEntry.Reference, Is.EqualTo("REF-001-UPDATED"));
    }

    [Test]
    public async Task Handle_ThrowsException_WhenJournalEntryNotFound()
    {
        // Arrange
        var journalEntryId = 999;
        var updateJournalEntryDto = new UpdateJournalEntryDto
        {
            Description = "Updated Entry",
            Lines = new List<CreateJournalEntryLineDto>
            {
                new() { AccountId = 1, DebitAmount = 1000, CreditAmount = 0 },
                new() { AccountId = 2, DebitAmount = 0, CreditAmount = 1000 }
            }
        };

        var command = new UpdateJournalEntryCommand(journalEntryId, updateJournalEntryDto);

        // Don't add any journal entries to the database, so the requested ID won't be found

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Handle_ThrowsException_WhenJournalEntryIsDeleted()
    {
        // Arrange
        var journalEntryId = 1;
        var updateJournalEntryDto = new UpdateJournalEntryDto
        {
            Description = "Updated Entry",
            Lines = new List<CreateJournalEntryLineDto>()
        };

        var command = new UpdateJournalEntryCommand(journalEntryId, updateJournalEntryDto);

        // Create a deleted journal entry in database
        var deletedJournalEntry = new JournalEntry
        {
            Id = journalEntryId,
            EntryNumber = "JE-DEL",
            TransactionDate = DateTime.UtcNow.Date,
            Description = "Deleted Entry",
            IsPosted = false,
            IsDeleted = true, // Entry is deleted
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = "test-user",
            UpdatedAt = DateTime.UtcNow,
            Lines = new List<JournalEntryLine>()
        };

        Context.JournalEntries.Add(deletedJournalEntry);
        Context.SaveChanges();

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Handle_ThrowsException_WhenJournalEntryIsPosted()
    {
        // Arrange
        var journalEntryId = 1;
        var updateJournalEntryDto = new UpdateJournalEntryDto
        {
            Description = "Updated Entry",
            Lines = new List<CreateJournalEntryLineDto>()
        };

        var command = new UpdateJournalEntryCommand(journalEntryId, updateJournalEntryDto);

        // Create a posted journal entry in database
        var postedJournalEntry = new JournalEntry
        {
            Id = journalEntryId,
            EntryNumber = "JE-POST",
            TransactionDate = DateTime.UtcNow.Date,
            Description = "Posted Entry",
            IsPosted = true, // Entry is posted
            IsDeleted = false,
            PostedAt = DateTime.UtcNow,
            PostedBy = "test-user",
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = "test-user",
            UpdatedAt = DateTime.UtcNow,
            Lines = new List<JournalEntryLine>()
        };

        Context.JournalEntries.Add(postedJournalEntry);
        Context.SaveChanges();

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Handle_ThrowsException_WhenJournalEntryIsNotBalanced()
    {
        // Arrange
        var journalEntryId = 1;
        var updateJournalEntryDto = new UpdateJournalEntryDto
        {
            Description = "Unbalanced Entry",
            Lines = new List<CreateJournalEntryLineDto>
            {
                new() { AccountId = 1, DebitAmount = 1000, CreditAmount = 0 },
                new() { AccountId = 2, DebitAmount = 0, CreditAmount = 500 } // Unbalanced
            }
        };

        var command = new UpdateJournalEntryCommand(journalEntryId, updateJournalEntryDto);

        // Create an unposted journal entry in database
        var existingJournalEntry = new JournalEntry
        {
            Id = journalEntryId,
            EntryNumber = "JE-UNBAL",
            TransactionDate = DateTime.UtcNow.Date,
            Description = "Original Entry",
            IsPosted = false,
            IsDeleted = false,
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = "test-user",
            UpdatedAt = DateTime.UtcNow,
            Lines = new List<JournalEntryLine>()
        };

        Context.JournalEntries.Add(existingJournalEntry);
        Context.SaveChanges();

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Handle_ThrowsException_WhenLineHasBothDebitAndCredit()
    {
        // Arrange
        var journalEntryId = 1;
        var updateJournalEntryDto = new UpdateJournalEntryDto
        {
            Description = "Invalid Entry",
            Lines = new List<CreateJournalEntryLineDto>
            {
                new() { AccountId = 1, DebitAmount = 1000, CreditAmount = 500 }, // Invalid line
                new() { AccountId = 2, DebitAmount = 0, CreditAmount = 500 }
            }
        };

        var command = new UpdateJournalEntryCommand(journalEntryId, updateJournalEntryDto);

        // Create an unposted journal entry in database
        var existingJournalEntry = new JournalEntry
        {
            Id = journalEntryId,
            EntryNumber = "JE-INVALID",
            TransactionDate = DateTime.UtcNow.Date,
            IsPosted = false,
            IsDeleted = false,
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = "test-user",
            UpdatedAt = DateTime.UtcNow,
            Lines = new List<JournalEntryLine>()
        };

        Context.JournalEntries.Add(existingJournalEntry);
        Context.SaveChanges();

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Handle_ThrowsException_WhenLineHasNeitherDebitNorCredit()
    {
        // Arrange
        var journalEntryId = 1;
        var updateJournalEntryDto = new UpdateJournalEntryDto
        {
            Description = "Invalid Entry",
            Lines = new List<CreateJournalEntryLineDto>
            {
                new() { AccountId = 1, DebitAmount = 0, CreditAmount = 0 }, // Invalid line
                new() { AccountId = 2, DebitAmount = 0, CreditAmount = 1000 }
            }
        };

        var command = new UpdateJournalEntryCommand(journalEntryId, updateJournalEntryDto);

        // Create an unposted journal entry in database
        var existingJournalEntry = new JournalEntry
        {
            Id = journalEntryId,
            EntryNumber = "JE-ZERO",
            TransactionDate = DateTime.UtcNow.Date,
            IsPosted = false,
            IsDeleted = false,
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = "test-user",
            UpdatedAt = DateTime.UtcNow,
            Lines = new List<JournalEntryLine>()
        };

        Context.JournalEntries.Add(existingJournalEntry);
        Context.SaveChanges();

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Handle_ThrowsException_WhenReferencedAccountDoesNotExist()
    {
        // Arrange
        var journalEntryId = 1;
        var updateJournalEntryDto = new UpdateJournalEntryDto
        {
            Description = "Entry with Missing Account",
            Lines = new List<CreateJournalEntryLineDto>
            {
                new() { AccountId = 1, DebitAmount = 1000, CreditAmount = 0 },
                new() { AccountId = 999, DebitAmount = 0, CreditAmount = 1000 } // Account doesn't exist
            }
        };

        var command = new UpdateJournalEntryCommand(journalEntryId, updateJournalEntryDto);

        // Create an unposted journal entry in database
        var existingJournalEntry = new JournalEntry
        {
            Id = journalEntryId,
            EntryNumber = "JE-MISSING",
            TransactionDate = DateTime.UtcNow.Date,
            IsPosted = false,
            IsDeleted = false,
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = "test-user",
            UpdatedAt = DateTime.UtcNow,
            Lines = new List<JournalEntryLine>()
        };

        Context.JournalEntries.Add(existingJournalEntry);
        Context.SaveChanges();

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Handle_AcceptsBalancedEntry_WithSmallRoundingDifference()
    {
        // Arrange
        var journalEntryId = 1;
        var updateJournalEntryDto = new UpdateJournalEntryDto
        {
            Description = "Entry with Small Rounding",
            Lines = new List<CreateJournalEntryLineDto>
            {
                new() { AccountId = 1, DebitAmount = 100.005m, CreditAmount = 0 },
                new() { AccountId = 2, DebitAmount = 0, CreditAmount = 100.00m } // Small rounding difference
            }
        };

        var command = new UpdateJournalEntryCommand(journalEntryId, updateJournalEntryDto);
        var currentUser = "test-user";

        // Create an unposted journal entry in database
        var existingJournalEntry = new JournalEntry
        {
            Id = journalEntryId,
            EntryNumber = "JE-ROUND",
            TransactionDate = DateTime.UtcNow.Date,
            Description = "Original Entry",
            IsPosted = false,
            IsDeleted = false,
            CreatedBy = currentUser,
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = currentUser,
            UpdatedAt = DateTime.UtcNow,
            Lines = new List<JournalEntryLine>()
        };

        Context.JournalEntries.Add(existingJournalEntry);
        Context.SaveChanges();

        CurrentUserServiceMock.Setup(x => x.GetCurrentUserForAudit()).Returns(currentUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Description, Is.EqualTo("Entry with Small Rounding"));
        
        // Verify the entity was actually updated in the database
        var updatedEntry = await Context.JournalEntries.FindAsync(journalEntryId);
        Assert.That(updatedEntry, Is.Not.Null);
        Assert.That(updatedEntry!.UpdatedBy, Is.EqualTo(currentUser));
    }

    [Test]
    public async Task Handle_SetsAuditFields_WhenUpdatingJournalEntry()
    {
        // Arrange
        var journalEntryId = 1;
        var updateJournalEntryDto = new UpdateJournalEntryDto
        {
            Description = "Audit Test Entry",
            Lines = new List<CreateJournalEntryLineDto>
            {
                new() { AccountId = 1, DebitAmount = 500, CreditAmount = 0 },
                new() { AccountId = 2, DebitAmount = 0, CreditAmount = 500 }
            }
        };

        var command = new UpdateJournalEntryCommand(journalEntryId, updateJournalEntryDto);
        var currentUser = "audit-user";

        // Create an unposted journal entry in database
        var existingJournalEntry = new JournalEntry
        {
            Id = journalEntryId,
            EntryNumber = "JE-AUDIT",
            TransactionDate = DateTime.UtcNow.Date,
            Description = "Original Entry",
            IsPosted = false,
            IsDeleted = false,
            CreatedBy = "original-user",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedBy = "original-user",
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            Lines = new List<JournalEntryLine>()
        };

        Context.JournalEntries.Add(existingJournalEntry);
        Context.SaveChanges();

        CurrentUserServiceMock.Setup(x => x.GetCurrentUserForAudit()).Returns(currentUser);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedEntry = await Context.JournalEntries.FindAsync(journalEntryId);
        Assert.That(updatedEntry, Is.Not.Null);
        Assert.That(updatedEntry!.UpdatedBy, Is.EqualTo(currentUser));
    }
}