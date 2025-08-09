using AccountingApi.Features.JournalEntries;
using AccountingApi.Infrastructure;
using AccountingApi.Models;
using AccountingApi.Services.CurrentUserService;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace AccountingApi.Tests.Features.JournalEntries;

public class DeleteJournalEntryHandlerTests
{
    private Mock<AccountingDbContext> _contextMock = null!;
    private Mock<ICurrentUserService> _currentUserServiceMock = null!;
    private DeleteJournalEntryCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AccountingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _contextMock = new Mock<AccountingDbContext>(options);
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        
        _handler = new DeleteJournalEntryCommandHandler(
            _contextMock.Object,
            _currentUserServiceMock.Object);
    }

    [Test]
    public async Task Handle_DeletesJournalEntryAndLines_WhenValidUnpostedEntry()
    {
        // Arrange
        var journalEntryId = 1;
        var command = new DeleteJournalEntryCommand(journalEntryId);
        var currentUser = "test-user";

        var journalEntryLines = new List<JournalEntryLine>
        {
            new() { Id = 1, JournalEntryId = journalEntryId, AccountId = 1, DebitAmount = 1000, CreditAmount = 0, IsDeleted = false },
            new() { Id = 2, JournalEntryId = journalEntryId, AccountId = 2, DebitAmount = 0, CreditAmount = 1000, IsDeleted = false }
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
        Assert.That(journalEntry.IsDeleted, Is.True);
        Assert.That(journalEntry.DeletedBy, Is.EqualTo(currentUser));
        Assert.That(journalEntry.UpdatedBy, Is.EqualTo(currentUser));
        Assert.That(journalEntry.DeletedAt, Is.Not.Null);
        Assert.That(journalEntry.UpdatedAt, Is.Not.Null);

        // Verify all lines are also deleted
        Assert.That(journalEntryLines.All(l => l.IsDeleted), Is.True);
        Assert.That(journalEntryLines.All(l => l.DeletedBy == currentUser), Is.True);
        Assert.That(journalEntryLines.All(l => l.UpdatedBy == currentUser), Is.True);
        Assert.That(journalEntryLines.All(l => l.DeletedAt != null), Is.True);
        Assert.That(journalEntryLines.All(l => l.UpdatedAt != null), Is.True);
        
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _currentUserServiceMock.Verify(x => x.GetCurrentUserForAudit(), Times.Once);
    }

    [Test]
    public async Task Handle_ReturnsFalse_WhenJournalEntryNotFound()
    {
        // Arrange
        var journalEntryId = 999;
        var command = new DeleteJournalEntryCommand(journalEntryId);

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

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.False);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_ThrowsException_WhenTryingToDeletePostedEntry()
    {
        // Arrange
        var journalEntryId = 1;
        var command = new DeleteJournalEntryCommand(journalEntryId);

        var journalEntry = new JournalEntry
        {
            Id = journalEntryId,
            Description = "Posted Journal Entry",
            IsPosted = true, // Entry is posted
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

        Assert.That(ex.Message, Does.Contain("Cannot delete a posted journal entry"));
        Assert.That(ex.Message, Does.Contain("Posted entries are immutable for audit purposes"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_DeletesEntryWithMultipleLines_SettingAllLinesDeleted()
    {
        // Arrange
        var journalEntryId = 1;
        var command = new DeleteJournalEntryCommand(journalEntryId);
        var currentUser = "test-user";

        var journalEntryLines = new List<JournalEntryLine>
        {
            new() { Id = 1, JournalEntryId = journalEntryId, AccountId = 1, DebitAmount = 1000, CreditAmount = 0, IsDeleted = false },
            new() { Id = 2, JournalEntryId = journalEntryId, AccountId = 2, DebitAmount = 0, CreditAmount = 500, IsDeleted = false },
            new() { Id = 3, JournalEntryId = journalEntryId, AccountId = 3, DebitAmount = 0, CreditAmount = 500, IsDeleted = false }
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
        Assert.That(journalEntry.IsDeleted, Is.True);
        
        // Verify all three lines are deleted with proper audit information
        Assert.That(journalEntryLines.Count, Is.EqualTo(3));
        Assert.That(journalEntryLines.All(l => l.IsDeleted), Is.True);
        Assert.That(journalEntryLines.All(l => l.DeletedBy == currentUser), Is.True);
        Assert.That(journalEntryLines.All(l => l.UpdatedBy == currentUser), Is.True);
        
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_DeletesEntryWithNoLines_OnlyMarkingEntryDeleted()
    {
        // Arrange
        var journalEntryId = 1;
        var command = new DeleteJournalEntryCommand(journalEntryId);
        var currentUser = "test-user";

        var journalEntry = new JournalEntry
        {
            Id = journalEntryId,
            Description = "Entry with no lines",
            IsPosted = false,
            IsDeleted = false,
            Lines = new List<JournalEntryLine>() // Empty list
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
        Assert.That(journalEntry.IsDeleted, Is.True);
        Assert.That(journalEntry.DeletedBy, Is.EqualTo(currentUser));
        Assert.That(journalEntry.UpdatedBy, Is.EqualTo(currentUser));
        
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_SetsCorrectAuditTimestamps_WhenDeletingEntry()
    {
        // Arrange
        var journalEntryId = 1;
        var command = new DeleteJournalEntryCommand(journalEntryId);
        var currentUser = "audit-user";
        var beforeDeletion = DateTime.UtcNow;

        var journalEntryLine = new JournalEntryLine
        {
            Id = 1,
            JournalEntryId = journalEntryId,
            AccountId = 1,
            DebitAmount = 1000,
            CreditAmount = 0,
            IsDeleted = false
        };

        var journalEntry = new JournalEntry
        {
            Id = journalEntryId,
            Description = "Audit Test Entry",
            IsPosted = false,
            IsDeleted = false,
            Lines = new List<JournalEntryLine> { journalEntryLine }
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
        var afterDeletion = DateTime.UtcNow;

        // Assert
        Assert.That(result, Is.True);
        
        // Check journal entry audit fields
        Assert.That(journalEntry.DeletedAt, Is.GreaterThanOrEqualTo(beforeDeletion));
        Assert.That(journalEntry.DeletedAt, Is.LessThanOrEqualTo(afterDeletion));
        Assert.That(journalEntry.UpdatedAt, Is.GreaterThanOrEqualTo(beforeDeletion));
        Assert.That(journalEntry.UpdatedAt, Is.LessThanOrEqualTo(afterDeletion));
        
        // Check journal entry line audit fields
        Assert.That(journalEntryLine.DeletedAt, Is.GreaterThanOrEqualTo(beforeDeletion));
        Assert.That(journalEntryLine.DeletedAt, Is.LessThanOrEqualTo(afterDeletion));
        Assert.That(journalEntryLine.UpdatedAt, Is.GreaterThanOrEqualTo(beforeDeletion));
        Assert.That(journalEntryLine.UpdatedAt, Is.LessThanOrEqualTo(afterDeletion));
        
        _currentUserServiceMock.Verify(x => x.GetCurrentUserForAudit(), Times.Once);
    }

    [Test]
    public async Task Handle_AllowsDeletionOfUnpostedEntry_RegardlessOfBalance()
    {
        // Arrange
        var journalEntryId = 1;
        var command = new DeleteJournalEntryCommand(journalEntryId);
        var currentUser = "test-user";

        // Create an unbalanced but unposted entry (should still allow deletion)
        var journalEntryLines = new List<JournalEntryLine>
        {
            new() { Id = 1, JournalEntryId = journalEntryId, AccountId = 1, DebitAmount = 1000, CreditAmount = 0, IsDeleted = false },
            new() { Id = 2, JournalEntryId = journalEntryId, AccountId = 2, DebitAmount = 0, CreditAmount = 500, IsDeleted = false } // Unbalanced
        };

        var journalEntry = new JournalEntry
        {
            Id = journalEntryId,
            Description = "Unbalanced Unposted Entry",
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
        Assert.That(journalEntry.IsDeleted, Is.True);
        Assert.That(journalEntryLines.All(l => l.IsDeleted), Is.True);
        
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}