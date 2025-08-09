using AccountingApi.DTOs;
using AccountingApi.Features.JournalEntries;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;
using AccountingApi.Models;
using AccountingApi.Services.CurrentUserService;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace AccountingApi.Tests.Features.JournalEntries;

public class UpdateJournalEntryHandlerTests
{
    private Mock<AccountingDbContext> _contextMock = null!;
    private Mock<JournalEntryMapper> _mapperMock = null!;
    private Mock<ICurrentUserService> _currentUserServiceMock = null!;
    private UpdateJournalEntryCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AccountingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _contextMock = new Mock<AccountingDbContext>(options);
        _mapperMock = new Mock<JournalEntryMapper>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        
        _handler = new UpdateJournalEntryCommandHandler(
            _contextMock.Object,
            _mapperMock.Object,
            _currentUserServiceMock.Object);
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

        var existingJournalEntry = new JournalEntry
        {
            Id = journalEntryId,
            Description = "Original Journal Entry",
            Reference = "REF-001",
            IsPosted = false,
            IsDeleted = false,
            Lines = new List<JournalEntryLine>
            {
                new() { Id = 1, AccountId = 1, DebitAmount = 1000, CreditAmount = 0 },
                new() { Id = 2, AccountId = 2, DebitAmount = 0, CreditAmount = 1000 }
            }
        };

        var expectedDto = new JournalEntryDto
        {
            Id = journalEntryId,
            Description = "Updated Journal Entry",
            Reference = "REF-001-UPDATED"
        };

        var accounts = new List<Account>
        {
            new() { Id = 1, AccountName = "Cash", IsDeleted = false },
            new() { Id = 2, AccountName = "Revenue", IsDeleted = false }
        };

        var journalEntries = new List<JournalEntry> { existingJournalEntry };
        var queryableJournalEntries = journalEntries.AsQueryable();

        var journalEntriesDbSetMock = new Mock<DbSet<JournalEntry>>();
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.Provider).Returns(queryableJournalEntries.Provider);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.Expression).Returns(queryableJournalEntries.Expression);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.ElementType).Returns(queryableJournalEntries.ElementType);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableJournalEntries.GetEnumerator());

        var queryableAccounts = accounts.AsQueryable();
        var accountsDbSetMock = new Mock<DbSet<Account>>();
        accountsDbSetMock.As<IQueryable<Account>>()
            .Setup(m => m.Provider).Returns(queryableAccounts.Provider);
        accountsDbSetMock.As<IQueryable<Account>>()
            .Setup(m => m.Expression).Returns(queryableAccounts.Expression);
        accountsDbSetMock.As<IQueryable<Account>>()
            .Setup(m => m.ElementType).Returns(queryableAccounts.ElementType);
        accountsDbSetMock.As<IQueryable<Account>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableAccounts.GetEnumerator());

        _contextMock.Setup(x => x.JournalEntries).Returns(journalEntriesDbSetMock.Object);
        _contextMock.Setup(x => x.Accounts).Returns(accountsDbSetMock.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _currentUserServiceMock.Setup(x => x.GetCurrentUserForAudit()).Returns(currentUser);
        _mapperMock.Setup(x => x.UpdateEntity(existingJournalEntry, updateJournalEntryDto, It.IsAny<List<Account>>()));
        _mapperMock.Setup(x => x.ToDto(existingJournalEntry)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Description, Is.EqualTo("Updated Journal Entry"));
        Assert.That(result.Reference, Is.EqualTo("REF-001-UPDATED"));
        Assert.That(existingJournalEntry.UpdatedBy, Is.EqualTo(currentUser));
        
        _mapperMock.Verify(x => x.UpdateEntity(existingJournalEntry, updateJournalEntryDto, It.IsAny<List<Account>>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _currentUserServiceMock.Verify(x => x.GetCurrentUserForAudit(), Times.Once);
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

        var journalEntries = new List<JournalEntry>();
        var queryableJournalEntries = journalEntries.AsQueryable();

        var journalEntriesDbSetMock = new Mock<DbSet<JournalEntry>>();
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.Provider).Returns(queryableJournalEntries.Provider);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.Expression).Returns(queryableJournalEntries.Expression);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.ElementType).Returns(queryableJournalEntries.ElementType);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableJournalEntries.GetEnumerator());

        _contextMock.Setup(x => x.JournalEntries).Returns(journalEntriesDbSetMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Journal entry not found"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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

        var existingJournalEntry = new JournalEntry
        {
            Id = journalEntryId,
            Description = "Deleted Entry",
            IsPosted = false,
            IsDeleted = true, // Entry is deleted
            Lines = new List<JournalEntryLine>()
        };

        var journalEntries = new List<JournalEntry> { existingJournalEntry };
        var queryableJournalEntries = journalEntries.Where(je => !je.IsDeleted).AsQueryable();

        var journalEntriesDbSetMock = new Mock<DbSet<JournalEntry>>();
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.Provider).Returns(queryableJournalEntries.Provider);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.Expression).Returns(queryableJournalEntries.Expression);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.ElementType).Returns(queryableJournalEntries.ElementType);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableJournalEntries.GetEnumerator());

        _contextMock.Setup(x => x.JournalEntries).Returns(journalEntriesDbSetMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Journal entry not found"));
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

        var existingJournalEntry = new JournalEntry
        {
            Id = journalEntryId,
            Description = "Posted Entry",
            IsPosted = true, // Entry is posted
            IsDeleted = false,
            Lines = new List<JournalEntryLine>()
        };

        var journalEntries = new List<JournalEntry> { existingJournalEntry };
        var queryableJournalEntries = journalEntries.AsQueryable();

        var journalEntriesDbSetMock = new Mock<DbSet<JournalEntry>>();
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.Provider).Returns(queryableJournalEntries.Provider);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.Expression).Returns(queryableJournalEntries.Expression);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.ElementType).Returns(queryableJournalEntries.ElementType);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableJournalEntries.GetEnumerator());

        _contextMock.Setup(x => x.JournalEntries).Returns(journalEntriesDbSetMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Cannot update a posted journal entry"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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

        var existingJournalEntry = new JournalEntry
        {
            Id = journalEntryId,
            Description = "Original Entry",
            IsPosted = false,
            IsDeleted = false,
            Lines = new List<JournalEntryLine>()
        };

        var journalEntries = new List<JournalEntry> { existingJournalEntry };
        var queryableJournalEntries = journalEntries.AsQueryable();

        var journalEntriesDbSetMock = new Mock<DbSet<JournalEntry>>();
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.Provider).Returns(queryableJournalEntries.Provider);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.Expression).Returns(queryableJournalEntries.Expression);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.ElementType).Returns(queryableJournalEntries.ElementType);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableJournalEntries.GetEnumerator());

        _contextMock.Setup(x => x.JournalEntries).Returns(journalEntriesDbSetMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Journal entry is not balanced"));
        Assert.That(ex.Message, Does.Contain("Debits: $1,000.00"));
        Assert.That(ex.Message, Does.Contain("Credits: $500.00"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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

        var existingJournalEntry = new JournalEntry
        {
            Id = journalEntryId,
            IsPosted = false,
            IsDeleted = false,
            Lines = new List<JournalEntryLine>()
        };

        var journalEntries = new List<JournalEntry> { existingJournalEntry };
        var queryableJournalEntries = journalEntries.AsQueryable();

        var journalEntriesDbSetMock = new Mock<DbSet<JournalEntry>>();
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.Provider).Returns(queryableJournalEntries.Provider);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.Expression).Returns(queryableJournalEntries.Expression);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.ElementType).Returns(queryableJournalEntries.ElementType);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableJournalEntries.GetEnumerator());

        _contextMock.Setup(x => x.JournalEntries).Returns(journalEntriesDbSetMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Each journal entry line must have either a debit amount or credit amount"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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

        var existingJournalEntry = new JournalEntry
        {
            Id = journalEntryId,
            IsPosted = false,
            IsDeleted = false,
            Lines = new List<JournalEntryLine>()
        };

        var journalEntries = new List<JournalEntry> { existingJournalEntry };
        var queryableJournalEntries = journalEntries.AsQueryable();

        var journalEntriesDbSetMock = new Mock<DbSet<JournalEntry>>();
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.Provider).Returns(queryableJournalEntries.Provider);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.Expression).Returns(queryableJournalEntries.Expression);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.ElementType).Returns(queryableJournalEntries.ElementType);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableJournalEntries.GetEnumerator());

        _contextMock.Setup(x => x.JournalEntries).Returns(journalEntriesDbSetMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Each journal entry line must have either a debit amount or credit amount"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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

        var existingJournalEntry = new JournalEntry
        {
            Id = journalEntryId,
            IsPosted = false,
            IsDeleted = false,
            Lines = new List<JournalEntryLine>()
        };

        var accounts = new List<Account>
        {
            new() { Id = 1, AccountName = "Cash", IsDeleted = false }
        };

        var journalEntries = new List<JournalEntry> { existingJournalEntry };
        var queryableJournalEntries = journalEntries.AsQueryable();

        var journalEntriesDbSetMock = new Mock<DbSet<JournalEntry>>();
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.Provider).Returns(queryableJournalEntries.Provider);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.Expression).Returns(queryableJournalEntries.Expression);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.ElementType).Returns(queryableJournalEntries.ElementType);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableJournalEntries.GetEnumerator());

        var queryableAccounts = accounts.AsQueryable();
        var accountsDbSetMock = new Mock<DbSet<Account>>();
        accountsDbSetMock.As<IQueryable<Account>>()
            .Setup(m => m.Provider).Returns(queryableAccounts.Provider);
        accountsDbSetMock.As<IQueryable<Account>>()
            .Setup(m => m.Expression).Returns(queryableAccounts.Expression);
        accountsDbSetMock.As<IQueryable<Account>>()
            .Setup(m => m.ElementType).Returns(queryableAccounts.ElementType);
        accountsDbSetMock.As<IQueryable<Account>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableAccounts.GetEnumerator());

        _contextMock.Setup(x => x.JournalEntries).Returns(journalEntriesDbSetMock.Object);
        _contextMock.Setup(x => x.Accounts).Returns(accountsDbSetMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("The following account IDs do not exist or are deleted: 999"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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

        var existingJournalEntry = new JournalEntry
        {
            Id = journalEntryId,
            Description = "Original Entry",
            IsPosted = false,
            IsDeleted = false,
            Lines = new List<JournalEntryLine>()
        };

        var expectedDto = new JournalEntryDto
        {
            Id = journalEntryId,
            Description = "Entry with Small Rounding"
        };

        var accounts = new List<Account>
        {
            new() { Id = 1, AccountName = "Cash", IsDeleted = false },
            new() { Id = 2, AccountName = "Revenue", IsDeleted = false }
        };

        var journalEntries = new List<JournalEntry> { existingJournalEntry };
        var queryableJournalEntries = journalEntries.AsQueryable();

        var journalEntriesDbSetMock = new Mock<DbSet<JournalEntry>>();
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.Provider).Returns(queryableJournalEntries.Provider);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.Expression).Returns(queryableJournalEntries.Expression);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.ElementType).Returns(queryableJournalEntries.ElementType);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableJournalEntries.GetEnumerator());

        var queryableAccounts = accounts.AsQueryable();
        var accountsDbSetMock = new Mock<DbSet<Account>>();
        accountsDbSetMock.As<IQueryable<Account>>()
            .Setup(m => m.Provider).Returns(queryableAccounts.Provider);
        accountsDbSetMock.As<IQueryable<Account>>()
            .Setup(m => m.Expression).Returns(queryableAccounts.Expression);
        accountsDbSetMock.As<IQueryable<Account>>()
            .Setup(m => m.ElementType).Returns(queryableAccounts.ElementType);
        accountsDbSetMock.As<IQueryable<Account>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableAccounts.GetEnumerator());

        _contextMock.Setup(x => x.JournalEntries).Returns(journalEntriesDbSetMock.Object);
        _contextMock.Setup(x => x.Accounts).Returns(accountsDbSetMock.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _currentUserServiceMock.Setup(x => x.GetCurrentUserForAudit()).Returns(currentUser);
        _mapperMock.Setup(x => x.UpdateEntity(existingJournalEntry, updateJournalEntryDto, It.IsAny<List<Account>>()));
        _mapperMock.Setup(x => x.ToDto(existingJournalEntry)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Description, Is.EqualTo("Entry with Small Rounding"));
        
        _mapperMock.Verify(x => x.UpdateEntity(existingJournalEntry, updateJournalEntryDto, It.IsAny<List<Account>>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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

        var existingJournalEntry = new JournalEntry
        {
            Id = journalEntryId,
            Description = "Original Entry",
            IsPosted = false,
            IsDeleted = false,
            Lines = new List<JournalEntryLine>()
        };

        var expectedDto = new JournalEntryDto
        {
            Id = journalEntryId,
            Description = "Audit Test Entry"
        };

        var accounts = new List<Account>
        {
            new() { Id = 1, AccountName = "Cash", IsDeleted = false },
            new() { Id = 2, AccountName = "Revenue", IsDeleted = false }
        };

        var journalEntries = new List<JournalEntry> { existingJournalEntry };
        var queryableJournalEntries = journalEntries.AsQueryable();

        var journalEntriesDbSetMock = new Mock<DbSet<JournalEntry>>();
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.Provider).Returns(queryableJournalEntries.Provider);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.Expression).Returns(queryableJournalEntries.Expression);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.ElementType).Returns(queryableJournalEntries.ElementType);
        journalEntriesDbSetMock.As<IQueryable<JournalEntry>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableJournalEntries.GetEnumerator());

        var queryableAccounts = accounts.AsQueryable();
        var accountsDbSetMock = new Mock<DbSet<Account>>();
        accountsDbSetMock.As<IQueryable<Account>>()
            .Setup(m => m.Provider).Returns(queryableAccounts.Provider);
        accountsDbSetMock.As<IQueryable<Account>>()
            .Setup(m => m.Expression).Returns(queryableAccounts.Expression);
        accountsDbSetMock.As<IQueryable<Account>>()
            .Setup(m => m.ElementType).Returns(queryableAccounts.ElementType);
        accountsDbSetMock.As<IQueryable<Account>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableAccounts.GetEnumerator());

        _contextMock.Setup(x => x.JournalEntries).Returns(journalEntriesDbSetMock.Object);
        _contextMock.Setup(x => x.Accounts).Returns(accountsDbSetMock.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _currentUserServiceMock.Setup(x => x.GetCurrentUserForAudit()).Returns(currentUser);
        _mapperMock.Setup(x => x.UpdateEntity(existingJournalEntry, updateJournalEntryDto, It.IsAny<List<Account>>()));
        _mapperMock.Setup(x => x.ToDto(existingJournalEntry)).Returns(expectedDto);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(existingJournalEntry.UpdatedBy, Is.EqualTo(currentUser));
        
        _currentUserServiceMock.Verify(x => x.GetCurrentUserForAudit(), Times.Once);
    }
}