using AccountingApi.Features.JournalEntries;
using AccountingApi.Infrastructure;
using AccountingApi.Models;
using AccountingApi.Services.CurrentUserService;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace AccountingApi.Tests.Features.JournalEntries;

public class PostJournalEntryHandlerTests
{
    private Mock<AccountingDbContext> _contextMock = null!;
    private Mock<ICurrentUserService> _currentUserServiceMock = null!;
    private PostJournalEntryCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AccountingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _contextMock = new Mock<AccountingDbContext>(options);
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        
        _handler = new PostJournalEntryCommandHandler(
            _contextMock.Object,
            _currentUserServiceMock.Object);
    }

    [Test]
    public async Task Handle_PostsJournalEntry_WhenValidBalancedUnpostedEntry()
    {
        // Arrange
        var journalEntryId = 1;
        var command = new PostJournalEntryCommand(journalEntryId);
        var currentUser = "test-user";

        var journalEntryLines = new List<JournalEntryLine>
        {
            new() { Id = 1, JournalEntryId = journalEntryId, AccountId = 1, DebitAmount = 1000, CreditAmount = 0 },
            new() { Id = 2, JournalEntryId = journalEntryId, AccountId = 2, DebitAmount = 0, CreditAmount = 1000 }
        };

        var journalEntry = new JournalEntry
        {
            Id = journalEntryId,
            Description = "Test Journal Entry",
            IsPosted = false,
            IsDeleted = false,
            Lines = journalEntryLines
        };

        var journalEntries = new List<JournalEntry> { journalEntry };
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
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _currentUserServiceMock.Setup(x => x.GetCurrentUserForAudit()).Returns(currentUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(journalEntry.IsPosted, Is.True);
        Assert.That(journalEntry.PostedAt, Is.Not.Null);
        Assert.That(journalEntry.PostedBy, Is.EqualTo(currentUser));
        Assert.That(journalEntry.UpdatedAt, Is.Not.Null);
        Assert.That(journalEntry.UpdatedBy, Is.EqualTo(currentUser));
        
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _currentUserServiceMock.Verify(x => x.GetCurrentUserForAudit(), Times.Once);
    }

    [Test]
    public async Task Handle_ThrowsException_WhenJournalEntryNotFound()
    {
        // Arrange
        var journalEntryId = 999;
        var command = new PostJournalEntryCommand(journalEntryId);

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

        Assert.That(ex.Message, Does.Contain("Journal entry not found or has been deleted"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_ThrowsException_WhenJournalEntryIsDeleted()
    {
        // Arrange
        var journalEntryId = 1;
        var command = new PostJournalEntryCommand(journalEntryId);

        var journalEntry = new JournalEntry
        {
            Id = journalEntryId,
            Description = "Deleted Journal Entry",
            IsPosted = false,
            IsDeleted = true, // Entry is deleted
            Lines = new List<JournalEntryLine>()
        };

        var journalEntries = new List<JournalEntry> { journalEntry };
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

        Assert.That(ex.Message, Does.Contain("Journal entry not found or has been deleted"));
    }

    [Test]
    public async Task Handle_ThrowsException_WhenJournalEntryAlreadyPosted()
    {
        // Arrange
        var journalEntryId = 1;
        var command = new PostJournalEntryCommand(journalEntryId);

        var journalEntry = new JournalEntry
        {
            Id = journalEntryId,
            Description = "Already Posted Entry",
            IsPosted = true, // Already posted
            IsDeleted = false,
            Lines = new List<JournalEntryLine>()
        };

        var journalEntries = new List<JournalEntry> { journalEntry };
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

        Assert.That(ex.Message, Does.Contain("Journal entry is already posted"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_ThrowsException_WhenJournalEntryIsNotBalanced()
    {
        // Arrange
        var journalEntryId = 1;
        var command = new PostJournalEntryCommand(journalEntryId);

        var journalEntryLines = new List<JournalEntryLine>
        {
            new() { Id = 1, JournalEntryId = journalEntryId, AccountId = 1, DebitAmount = 1000, CreditAmount = 0 },
            new() { Id = 2, JournalEntryId = journalEntryId, AccountId = 2, DebitAmount = 0, CreditAmount = 500 } // Unbalanced
        };

        var journalEntry = new JournalEntry
        {
            Id = journalEntryId,
            Description = "Unbalanced Journal Entry",
            IsPosted = false,
            IsDeleted = false,
            Lines = journalEntryLines
        };

        var journalEntries = new List<JournalEntry> { journalEntry };
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

        Assert.That(ex.Message, Does.Contain("Cannot post unbalanced journal entry"));
        Assert.That(ex.Message, Does.Contain("Debits: $1,000.00"));
        Assert.That(ex.Message, Does.Contain("Credits: $500.00"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_ThrowsException_WhenLineHasBothDebitAndCredit()
    {
        // Arrange
        var journalEntryId = 1;
        var command = new PostJournalEntryCommand(journalEntryId);

        var journalEntryLines = new List<JournalEntryLine>
        {
            new() { Id = 1, JournalEntryId = journalEntryId, AccountId = 1, DebitAmount = 1000, CreditAmount = 500 }, // Invalid line
            new() { Id = 2, JournalEntryId = journalEntryId, AccountId = 2, DebitAmount = 0, CreditAmount = 500 }
        };

        var journalEntry = new JournalEntry
        {
            Id = journalEntryId,
            Description = "Invalid Line Journal Entry",
            IsPosted = false,
            IsDeleted = false,
            Lines = journalEntryLines
        };

        var journalEntries = new List<JournalEntry> { journalEntry };
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

        Assert.That(ex.Message, Does.Contain("Cannot post journal entry with invalid line amounts"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_ThrowsException_WhenLineHasNeitherDebitNorCredit()
    {
        // Arrange
        var journalEntryId = 1;
        var command = new PostJournalEntryCommand(journalEntryId);

        var journalEntryLines = new List<JournalEntryLine>
        {
            new() { Id = 1, JournalEntryId = journalEntryId, AccountId = 1, DebitAmount = 0, CreditAmount = 0 }, // Invalid line
            new() { Id = 2, JournalEntryId = journalEntryId, AccountId = 2, DebitAmount = 0, CreditAmount = 1000 }
        };

        var journalEntry = new JournalEntry
        {
            Id = journalEntryId,
            Description = "Invalid Line Journal Entry",
            IsPosted = false,
            IsDeleted = false,
            Lines = journalEntryLines
        };

        var journalEntries = new List<JournalEntry> { journalEntry };
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

        Assert.That(ex.Message, Does.Contain("Cannot post journal entry with invalid line amounts"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_AcceptsBalancedEntry_WithSmallRoundingDifference()
    {
        // Arrange
        var journalEntryId = 1;
        var command = new PostJournalEntryCommand(journalEntryId);
        var currentUser = "test-user";

        var journalEntryLines = new List<JournalEntryLine>
        {
            new() { Id = 1, JournalEntryId = journalEntryId, AccountId = 1, DebitAmount = 100.005m, CreditAmount = 0 },
            new() { Id = 2, JournalEntryId = journalEntryId, AccountId = 2, DebitAmount = 0, CreditAmount = 100.00m } // Small rounding difference
        };

        var journalEntry = new JournalEntry
        {
            Id = journalEntryId,
            Description = "Rounding Difference Entry",
            IsPosted = false,
            IsDeleted = false,
            Lines = journalEntryLines
        };

        var journalEntries = new List<JournalEntry> { journalEntry };
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
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _currentUserServiceMock.Setup(x => x.GetCurrentUserForAudit()).Returns(currentUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(journalEntry.IsPosted, Is.True);
        Assert.That(journalEntry.PostedBy, Is.EqualTo(currentUser));
        
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_SetsCorrectAuditTimestamps_WhenPostingEntry()
    {
        // Arrange
        var journalEntryId = 1;
        var command = new PostJournalEntryCommand(journalEntryId);
        var currentUser = "audit-user";
        var beforePosting = DateTime.UtcNow;

        var journalEntryLines = new List<JournalEntryLine>
        {
            new() { Id = 1, JournalEntryId = journalEntryId, AccountId = 1, DebitAmount = 500, CreditAmount = 0 },
            new() { Id = 2, JournalEntryId = journalEntryId, AccountId = 2, DebitAmount = 0, CreditAmount = 500 }
        };

        var journalEntry = new JournalEntry
        {
            Id = journalEntryId,
            Description = "Audit Test Entry",
            IsPosted = false,
            IsDeleted = false,
            Lines = journalEntryLines
        };

        var journalEntries = new List<JournalEntry> { journalEntry };
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
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _currentUserServiceMock.Setup(x => x.GetCurrentUserForAudit()).Returns(currentUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var afterPosting = DateTime.UtcNow;

        // Assert
        Assert.That(result, Is.True);
        Assert.That(journalEntry.IsPosted, Is.True);
        Assert.That(journalEntry.PostedBy, Is.EqualTo(currentUser));
        Assert.That(journalEntry.UpdatedBy, Is.EqualTo(currentUser));
        Assert.That(journalEntry.PostedAt, Is.GreaterThanOrEqualTo(beforePosting));
        Assert.That(journalEntry.PostedAt, Is.LessThanOrEqualTo(afterPosting));
        Assert.That(journalEntry.UpdatedAt, Is.GreaterThanOrEqualTo(beforePosting));
        Assert.That(journalEntry.UpdatedAt, Is.LessThanOrEqualTo(afterPosting));
        
        _currentUserServiceMock.Verify(x => x.GetCurrentUserForAudit(), Times.Once);
    }

    [Test]
    public async Task Handle_PostsEntryWithMultipleLines_WhenAllLinesValid()
    {
        // Arrange
        var journalEntryId = 1;
        var command = new PostJournalEntryCommand(journalEntryId);
        var currentUser = "test-user";

        var journalEntryLines = new List<JournalEntryLine>
        {
            new() { Id = 1, JournalEntryId = journalEntryId, AccountId = 1, DebitAmount = 1000, CreditAmount = 0 },
            new() { Id = 2, JournalEntryId = journalEntryId, AccountId = 2, DebitAmount = 500, CreditAmount = 0 },
            new() { Id = 3, JournalEntryId = journalEntryId, AccountId = 3, DebitAmount = 0, CreditAmount = 1500 }
        };

        var journalEntry = new JournalEntry
        {
            Id = journalEntryId,
            Description = "Multi-line Journal Entry",
            IsPosted = false,
            IsDeleted = false,
            Lines = journalEntryLines
        };

        var journalEntries = new List<JournalEntry> { journalEntry };
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
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _currentUserServiceMock.Setup(x => x.GetCurrentUserForAudit()).Returns(currentUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(journalEntry.IsPosted, Is.True);
        Assert.That(journalEntry.PostedBy, Is.EqualTo(currentUser));
        Assert.That(journalEntry.UpdatedBy, Is.EqualTo(currentUser));
        
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}