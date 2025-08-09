using AccountingApi.Features.JournalEntries;
using AccountingApi.Infrastructure;
using AccountingApi.Models;
using AccountingApi.Services.CurrentUserService;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace AccountingApi.Tests.Features.JournalEntries;

public class DeleteJournalEntryLineHandlerTests
{
    private Mock<AccountingDbContext> _contextMock = null!;
    private Mock<ICurrentUserService> _currentUserServiceMock = null!;
    private DeleteJournalEntryLineCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AccountingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _contextMock = new Mock<AccountingDbContext>(options);
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        
        _handler = new DeleteJournalEntryLineCommandHandler(
            _contextMock.Object,
            _currentUserServiceMock.Object);
    }

    [Test]
    public async Task Handle_DeletesJournalEntryLine_WhenValidUnpostedEntryWithMultipleLines()
    {
        // Arrange
        var lineId = 1;
        var journalEntryId = 1;
        var command = new DeleteJournalEntryLineCommand(lineId);
        var currentUser = "test-user";

        var journalEntry = new JournalEntry
        {
            Id = journalEntryId,
            Description = "Test Journal Entry",
            IsPosted = false,
            IsDeleted = false
        };

        var lineToDelete = new JournalEntryLine
        {
            Id = lineId,
            JournalEntryId = journalEntryId,
            JournalEntry = journalEntry,
            AccountId = 1,
            DebitAmount = 500,
            CreditAmount = 0,
            IsDeleted = false
        };

        var remainingLines = new List<JournalEntryLine>
        {
            new() { Id = 2, JournalEntryId = journalEntryId, AccountId = 2, DebitAmount = 500, CreditAmount = 0, IsDeleted = false },
            new() { Id = 3, JournalEntryId = journalEntryId, AccountId = 3, DebitAmount = 0, CreditAmount = 1000, IsDeleted = false }
        };

        var allLines = new List<JournalEntryLine> { lineToDelete }.Concat(remainingLines).ToList();

        var queryableLines = allLines.AsQueryable();
        var linesDbSetMock = new Mock<DbSet<JournalEntryLine>>();
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.Provider).Returns(queryableLines.Provider);
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.Expression).Returns(queryableLines.Expression);
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.ElementType).Returns(queryableLines.ElementType);
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableLines.GetEnumerator());

        _contextMock.Setup(x => x.JournalEntryLines).Returns(linesDbSetMock.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _currentUserServiceMock.Setup(x => x.GetCurrentUserForAudit()).Returns(currentUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(lineToDelete.IsDeleted, Is.True);
        Assert.That(lineToDelete.DeletedBy, Is.EqualTo(currentUser));
        Assert.That(lineToDelete.UpdatedBy, Is.EqualTo(currentUser));
        Assert.That(lineToDelete.DeletedAt, Is.Not.Null);
        Assert.That(lineToDelete.UpdatedAt, Is.Not.Null);
        
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _currentUserServiceMock.Verify(x => x.GetCurrentUserForAudit(), Times.Once);
    }

    [Test]
    public async Task Handle_ReturnsFalse_WhenLineNotFound()
    {
        // Arrange
        var lineId = 999;
        var command = new DeleteJournalEntryLineCommand(lineId);

        var lines = new List<JournalEntryLine>();
        var queryableLines = lines.AsQueryable();

        var linesDbSetMock = new Mock<DbSet<JournalEntryLine>>();
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.Provider).Returns(queryableLines.Provider);
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.Expression).Returns(queryableLines.Expression);
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.ElementType).Returns(queryableLines.ElementType);
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableLines.GetEnumerator());

        _contextMock.Setup(x => x.JournalEntryLines).Returns(linesDbSetMock.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.False);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_ThrowsException_WhenTryingToDeleteFromPostedEntry()
    {
        // Arrange
        var lineId = 1;
        var journalEntryId = 1;
        var command = new DeleteJournalEntryLineCommand(lineId);

        var journalEntry = new JournalEntry
        {
            Id = journalEntryId,
            Description = "Posted Journal Entry",
            IsPosted = true, // Entry is posted
            IsDeleted = false
        };

        var lineToDelete = new JournalEntryLine
        {
            Id = lineId,
            JournalEntryId = journalEntryId,
            JournalEntry = journalEntry,
            AccountId = 1,
            DebitAmount = 1000,
            CreditAmount = 0,
            IsDeleted = false
        };

        var lines = new List<JournalEntryLine> { lineToDelete };
        var queryableLines = lines.AsQueryable();

        var linesDbSetMock = new Mock<DbSet<JournalEntryLine>>();
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.Provider).Returns(queryableLines.Provider);
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.Expression).Returns(queryableLines.Expression);
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.ElementType).Returns(queryableLines.ElementType);
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableLines.GetEnumerator());

        _contextMock.Setup(x => x.JournalEntryLines).Returns(linesDbSetMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Cannot delete lines from a posted journal entry"));
        Assert.That(ex.Message, Does.Contain("Posted entries are immutable for audit purposes"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_ThrowsException_WhenTryingToDeleteLastRemainingLine()
    {
        // Arrange
        var lineId = 1;
        var journalEntryId = 1;
        var command = new DeleteJournalEntryLineCommand(lineId);

        var journalEntry = new JournalEntry
        {
            Id = journalEntryId,
            Description = "Journal Entry with Single Line",
            IsPosted = false,
            IsDeleted = false
        };

        var lineToDelete = new JournalEntryLine
        {
            Id = lineId,
            JournalEntryId = journalEntryId,
            JournalEntry = journalEntry,
            AccountId = 1,
            DebitAmount = 1000,
            CreditAmount = 0,
            IsDeleted = false
        };

        var lines = new List<JournalEntryLine> { lineToDelete };
        var queryableLines = lines.AsQueryable();

        var linesDbSetMock = new Mock<DbSet<JournalEntryLine>>();
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.Provider).Returns(queryableLines.Provider);
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.Expression).Returns(queryableLines.Expression);
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.ElementType).Returns(queryableLines.ElementType);
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableLines.GetEnumerator());

        _contextMock.Setup(x => x.JournalEntryLines).Returns(linesDbSetMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Cannot delete the last remaining line from a journal entry"));
        Assert.That(ex.Message, Does.Contain("Delete the entire journal entry instead"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_ThrowsException_WhenDeletionWouldLeaveEntryUnbalanced()
    {
        // Arrange
        var lineId = 1;
        var journalEntryId = 1;
        var command = new DeleteJournalEntryLineCommand(lineId);

        var journalEntry = new JournalEntry
        {
            Id = journalEntryId,
            Description = "Journal Entry",
            IsPosted = false,
            IsDeleted = false
        };

        var lineToDelete = new JournalEntryLine
        {
            Id = lineId,
            JournalEntryId = journalEntryId,
            JournalEntry = journalEntry,
            AccountId = 1,
            DebitAmount = 1000,
            CreditAmount = 0,
            IsDeleted = false
        };

        var remainingLines = new List<JournalEntryLine>
        {
            new() { Id = 2, JournalEntryId = journalEntryId, AccountId = 2, DebitAmount = 0, CreditAmount = 500, IsDeleted = false }
            // After deleting line 1 (Debit 1000), only Credit 500 remains - unbalanced
        };

        var allLines = new List<JournalEntryLine> { lineToDelete }.Concat(remainingLines).ToList();

        var queryableLines = allLines.AsQueryable();
        var linesDbSetMock = new Mock<DbSet<JournalEntryLine>>();
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.Provider).Returns(queryableLines.Provider);
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.Expression).Returns(queryableLines.Expression);
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.ElementType).Returns(queryableLines.ElementType);
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableLines.GetEnumerator());

        _contextMock.Setup(x => x.JournalEntryLines).Returns(linesDbSetMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Cannot delete this journal entry line as it would leave the journal entry unbalanced"));
        Assert.That(ex.Message, Does.Contain("The sum of debits must equal the sum of credits"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_AllowsDeletion_WhenRemainingLinesStayBalanced()
    {
        // Arrange
        var lineId = 1;
        var journalEntryId = 1;
        var command = new DeleteJournalEntryLineCommand(lineId);
        var currentUser = "test-user";

        var journalEntry = new JournalEntry
        {
            Id = journalEntryId,
            Description = "Balanced Journal Entry",
            IsPosted = false,
            IsDeleted = false
        };

        var lineToDelete = new JournalEntryLine
        {
            Id = lineId,
            JournalEntryId = journalEntryId,
            JournalEntry = journalEntry,
            AccountId = 1,
            DebitAmount = 500,
            CreditAmount = 0,
            IsDeleted = false
        };

        var remainingLines = new List<JournalEntryLine>
        {
            new() { Id = 2, JournalEntryId = journalEntryId, AccountId = 2, DebitAmount = 1000, CreditAmount = 0, IsDeleted = false },
            new() { Id = 3, JournalEntryId = journalEntryId, AccountId = 3, DebitAmount = 0, CreditAmount = 1000, IsDeleted = false }
            // After deleting line 1 (Debit 500), remaining: Debit 1000 = Credit 1000 (balanced)
        };

        var allLines = new List<JournalEntryLine> { lineToDelete }.Concat(remainingLines).ToList();

        var queryableLines = allLines.AsQueryable();
        var linesDbSetMock = new Mock<DbSet<JournalEntryLine>>();
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.Provider).Returns(queryableLines.Provider);
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.Expression).Returns(queryableLines.Expression);
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.ElementType).Returns(queryableLines.ElementType);
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableLines.GetEnumerator());

        _contextMock.Setup(x => x.JournalEntryLines).Returns(linesDbSetMock.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _currentUserServiceMock.Setup(x => x.GetCurrentUserForAudit()).Returns(currentUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(lineToDelete.IsDeleted, Is.True);
        Assert.That(lineToDelete.DeletedBy, Is.EqualTo(currentUser));
        
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_IgnoresAlreadyDeletedLines_WhenCheckingRemainingLines()
    {
        // Arrange
        var lineId = 1;
        var journalEntryId = 1;
        var command = new DeleteJournalEntryLineCommand(lineId);
        var currentUser = "test-user";

        var journalEntry = new JournalEntry
        {
            Id = journalEntryId,
            Description = "Entry with Deleted Lines",
            IsPosted = false,
            IsDeleted = false
        };

        var lineToDelete = new JournalEntryLine
        {
            Id = lineId,
            JournalEntryId = journalEntryId,
            JournalEntry = journalEntry,
            AccountId = 1,
            DebitAmount = 200,
            CreditAmount = 0,
            IsDeleted = false
        };

        var remainingLines = new List<JournalEntryLine>
        {
            new() { Id = 2, JournalEntryId = journalEntryId, AccountId = 2, DebitAmount = 800, CreditAmount = 0, IsDeleted = false },
            new() { Id = 3, JournalEntryId = journalEntryId, AccountId = 3, DebitAmount = 0, CreditAmount = 800, IsDeleted = false },
            new() { Id = 4, JournalEntryId = journalEntryId, AccountId = 4, DebitAmount = 500, CreditAmount = 0, IsDeleted = true } // Already deleted, should be ignored
        };

        var allLines = new List<JournalEntryLine> { lineToDelete }.Concat(remainingLines).ToList();

        var queryableLines = allLines.AsQueryable();
        var linesDbSetMock = new Mock<DbSet<JournalEntryLine>>();
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.Provider).Returns(queryableLines.Provider);
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.Expression).Returns(queryableLines.Expression);
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.ElementType).Returns(queryableLines.ElementType);
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableLines.GetEnumerator());

        _contextMock.Setup(x => x.JournalEntryLines).Returns(linesDbSetMock.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _currentUserServiceMock.Setup(x => x.GetCurrentUserForAudit()).Returns(currentUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(lineToDelete.IsDeleted, Is.True);
        
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_SetsCorrectAuditTimestamps_WhenDeletingLine()
    {
        // Arrange
        var lineId = 1;
        var journalEntryId = 1;
        var command = new DeleteJournalEntryLineCommand(lineId);
        var currentUser = "audit-user";
        var beforeDeletion = DateTime.UtcNow;

        var journalEntry = new JournalEntry
        {
            Id = journalEntryId,
            Description = "Audit Test Entry",
            IsPosted = false,
            IsDeleted = false
        };

        var lineToDelete = new JournalEntryLine
        {
            Id = lineId,
            JournalEntryId = journalEntryId,
            JournalEntry = journalEntry,
            AccountId = 1,
            DebitAmount = 1000,
            CreditAmount = 0,
            IsDeleted = false
        };

        var remainingLines = new List<JournalEntryLine>
        {
            new() { Id = 2, JournalEntryId = journalEntryId, AccountId = 2, DebitAmount = 0, CreditAmount = 1000, IsDeleted = false }
        };

        var allLines = new List<JournalEntryLine> { lineToDelete }.Concat(remainingLines).ToList();

        var queryableLines = allLines.AsQueryable();
        var linesDbSetMock = new Mock<DbSet<JournalEntryLine>>();
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.Provider).Returns(queryableLines.Provider);
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.Expression).Returns(queryableLines.Expression);
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.ElementType).Returns(queryableLines.ElementType);
        linesDbSetMock.As<IQueryable<JournalEntryLine>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableLines.GetEnumerator());

        _contextMock.Setup(x => x.JournalEntryLines).Returns(linesDbSetMock.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _currentUserServiceMock.Setup(x => x.GetCurrentUserForAudit()).Returns(currentUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var afterDeletion = DateTime.UtcNow;

        // Assert
        Assert.That(result, Is.True);
        Assert.That(lineToDelete.DeletedBy, Is.EqualTo(currentUser));
        Assert.That(lineToDelete.UpdatedBy, Is.EqualTo(currentUser));
        Assert.That(lineToDelete.DeletedAt, Is.GreaterThanOrEqualTo(beforeDeletion));
        Assert.That(lineToDelete.DeletedAt, Is.LessThanOrEqualTo(afterDeletion));
        Assert.That(lineToDelete.UpdatedAt, Is.GreaterThanOrEqualTo(beforeDeletion));
        Assert.That(lineToDelete.UpdatedAt, Is.LessThanOrEqualTo(afterDeletion));
        
        _currentUserServiceMock.Verify(x => x.GetCurrentUserForAudit(), Times.Once);
    }
}