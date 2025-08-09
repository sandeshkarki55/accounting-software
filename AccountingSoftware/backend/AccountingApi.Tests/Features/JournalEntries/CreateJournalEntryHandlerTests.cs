using AccountingApi.DTOs;
using AccountingApi.Features.JournalEntries;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;
using AccountingApi.Models;
using AccountingApi.Services.CurrentUserService;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace AccountingApi.Tests.Features.JournalEntries;

public class CreateJournalEntryHandlerTests
{
    private Mock<AccountingDbContext> _contextMock = null!;
    private Mock<JournalEntryMapper> _mapperMock = null!;
    private Mock<ICurrentUserService> _currentUserServiceMock = null!;
    private CreateJournalEntryCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AccountingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _contextMock = new Mock<AccountingDbContext>(options);
        _mapperMock = new Mock<JournalEntryMapper>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        
        _handler = new CreateJournalEntryCommandHandler(
            _contextMock.Object,
            _mapperMock.Object,
            _currentUserServiceMock.Object);
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
        var currentUser = "test-user";

        var journalEntryEntity = new JournalEntry
        {
            Id = 1,
            TransactionDate = createJournalEntryDto.TransactionDate,
            Description = createJournalEntryDto.Description,
            Reference = createJournalEntryDto.Reference,
            CreatedBy = currentUser,
            UpdatedBy = currentUser
        };

        var expectedDto = new JournalEntryDto
        {
            Id = 1,
            TransactionDate = createJournalEntryDto.TransactionDate,
            Description = createJournalEntryDto.Description,
            Reference = createJournalEntryDto.Reference
        };

        var accounts = new List<Account>
        {
            new() { Id = 1, AccountName = "Cash", IsDeleted = false },
            new() { Id = 2, AccountName = "Revenue", IsDeleted = false }
        };

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

        var journalEntriesDbSetMock = new Mock<DbSet<JournalEntry>>();

        _currentUserServiceMock.Setup(x => x.GetCurrentUserForAudit()).Returns(currentUser);
        _mapperMock.Setup(x => x.ToEntity(createJournalEntryDto)).Returns(journalEntryEntity);
        _mapperMock.Setup(x => x.ToDto(It.IsAny<JournalEntry>())).Returns(expectedDto);

        _contextMock.Setup(x => x.Accounts).Returns(accountsDbSetMock.Object);
        _contextMock.Setup(x => x.JournalEntries).Returns(journalEntriesDbSetMock.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Description, Is.EqualTo("Test Journal Entry"));
        Assert.That(result.Reference, Is.EqualTo("REF-001"));
        
        journalEntriesDbSetMock.Verify(x => x.Add(It.IsAny<JournalEntry>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _currentUserServiceMock.Verify(x => x.GetCurrentUserForAudit(), Times.Once);
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
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Journal entry is not balanced"));
        Assert.That(ex.Message, Does.Contain("Debits: $1,000.00"));
        Assert.That(ex.Message, Does.Contain("Credits: $500.00"));
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
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Each journal entry line must have either a debit amount or credit amount"));
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
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Each journal entry line must have either a debit amount or credit amount"));
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

        // Only account ID 1 exists
        var accounts = new List<Account>
        {
            new() { Id = 1, AccountName = "Cash", IsDeleted = false }
        };

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

        _contextMock.Setup(x => x.Accounts).Returns(accountsDbSetMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("The following account IDs do not exist or are deleted: 999"));
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

        // Account 2 is deleted
        var accounts = new List<Account>
        {
            new() { Id = 1, AccountName = "Cash", IsDeleted = false },
            new() { Id = 2, AccountName = "Revenue", IsDeleted = true }
        };

        var queryableAccounts = accounts.Where(a => !a.IsDeleted).AsQueryable();
        var accountsDbSetMock = new Mock<DbSet<Account>>();
        accountsDbSetMock.As<IQueryable<Account>>()
            .Setup(m => m.Provider).Returns(queryableAccounts.Provider);
        accountsDbSetMock.As<IQueryable<Account>>()
            .Setup(m => m.Expression).Returns(queryableAccounts.Expression);
        accountsDbSetMock.As<IQueryable<Account>>()
            .Setup(m => m.ElementType).Returns(queryableAccounts.ElementType);
        accountsDbSetMock.As<IQueryable<Account>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableAccounts.GetEnumerator());

        _contextMock.Setup(x => x.Accounts).Returns(accountsDbSetMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("The following account IDs do not exist or are deleted: 2"));
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
        var currentUser = "test-user";

        var journalEntryEntity = new JournalEntry
        {
            Id = 1,
            Description = createJournalEntryDto.Description,
            CreatedBy = currentUser,
            UpdatedBy = currentUser
        };

        var expectedDto = new JournalEntryDto
        {
            Id = 1,
            Description = createJournalEntryDto.Description
        };

        var accounts = new List<Account>
        {
            new() { Id = 1, AccountName = "Cash", IsDeleted = false },
            new() { Id = 2, AccountName = "Revenue", IsDeleted = false }
        };

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

        var journalEntriesDbSetMock = new Mock<DbSet<JournalEntry>>();

        _currentUserServiceMock.Setup(x => x.GetCurrentUserForAudit()).Returns(currentUser);
        _mapperMock.Setup(x => x.ToEntity(createJournalEntryDto)).Returns(journalEntryEntity);
        _mapperMock.Setup(x => x.ToDto(It.IsAny<JournalEntry>())).Returns(expectedDto);

        _contextMock.Setup(x => x.Accounts).Returns(accountsDbSetMock.Object);
        _contextMock.Setup(x => x.JournalEntries).Returns(journalEntriesDbSetMock.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Description, Is.EqualTo("Journal Entry with Small Rounding"));
        
        journalEntriesDbSetMock.Verify(x => x.Add(It.IsAny<JournalEntry>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
        var currentUser = "audit-user";

        var journalEntryEntity = new JournalEntry
        {
            Description = createJournalEntryDto.Description
        };

        var expectedDto = new JournalEntryDto
        {
            Description = createJournalEntryDto.Description
        };

        var accounts = new List<Account>
        {
            new() { Id = 1, AccountName = "Cash", IsDeleted = false },
            new() { Id = 2, AccountName = "Revenue", IsDeleted = false }
        };

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

        var journalEntriesDbSetMock = new Mock<DbSet<JournalEntry>>();

        _currentUserServiceMock.Setup(x => x.GetCurrentUserForAudit()).Returns(currentUser);
        _mapperMock.Setup(x => x.ToEntity(createJournalEntryDto)).Returns(journalEntryEntity);
        _mapperMock.Setup(x => x.ToDto(It.IsAny<JournalEntry>())).Returns(expectedDto);

        _contextMock.Setup(x => x.Accounts).Returns(accountsDbSetMock.Object);
        _contextMock.Setup(x => x.JournalEntries).Returns(journalEntriesDbSetMock.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(journalEntryEntity.CreatedBy, Is.EqualTo(currentUser));
        Assert.That(journalEntryEntity.UpdatedBy, Is.EqualTo(currentUser));
        
        _currentUserServiceMock.Verify(x => x.GetCurrentUserForAudit(), Times.Once);
    }
}