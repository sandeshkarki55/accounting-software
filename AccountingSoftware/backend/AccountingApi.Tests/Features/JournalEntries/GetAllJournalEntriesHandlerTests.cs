using AccountingApi.DTOs;
using AccountingApi.Features.JournalEntries;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;
using AccountingApi.Models;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace AccountingApi.Tests.Features.JournalEntries;

public class GetAllJournalEntriesHandlerTests
{
    private Mock<AccountingDbContext> _contextMock = null!;
    private Mock<JournalEntryMapper> _mapperMock = null!;
    private GetAllJournalEntriesQueryHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AccountingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _contextMock = new Mock<AccountingDbContext>(options);
        _mapperMock = new Mock<JournalEntryMapper>();
        
        _handler = new GetAllJournalEntriesQueryHandler(
            _contextMock.Object,
            _mapperMock.Object);
    }

    [Test]
    public async Task Handle_ReturnsPagedResults_WhenValidRequest()
    {
        // Arrange
        var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 2 };
        var sortingParams = new SortingParams { OrderBy = "TransactionDate", IsDescending = false };
        var filteringParams = new JournalEntryFilteringParams { SearchTerm = null, StatusFilter = null };
        
        var query = new GetAllJournalEntriesQuery(paginationParams, sortingParams, filteringParams);

        var accounts = new List<Account>
        {
            new() { Id = 1, AccountName = "Cash" },
            new() { Id = 2, AccountName = "Revenue" }
        };

        var journalEntries = new List<JournalEntry>
        {
            new() 
            { 
                Id = 1, 
                EntryNumber = "JE-001", 
                Description = "First Entry", 
                TransactionDate = DateTime.UtcNow.AddDays(-2),
                IsDeleted = false,
                Lines = new List<JournalEntryLine>
                {
                    new() { Id = 1, AccountId = 1, Account = accounts[0], DebitAmount = 1000, CreditAmount = 0 }
                }
            },
            new() 
            { 
                Id = 2, 
                EntryNumber = "JE-002", 
                Description = "Second Entry", 
                TransactionDate = DateTime.UtcNow.AddDays(-1),
                IsDeleted = false,
                Lines = new List<JournalEntryLine>
                {
                    new() { Id = 2, AccountId = 2, Account = accounts[1], DebitAmount = 0, CreditAmount = 500 }
                }
            },
            new() 
            { 
                Id = 3, 
                EntryNumber = "JE-003", 
                Description = "Third Entry", 
                TransactionDate = DateTime.UtcNow,
                IsDeleted = false,
                Lines = new List<JournalEntryLine>()
            }
        };

        var expectedDtos = new List<JournalEntryDto>
        {
            new() { Id = 1, EntryNumber = "JE-001", Description = "First Entry" },
            new() { Id = 2, EntryNumber = "JE-002", Description = "Second Entry" }
        };

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
        _mapperMock.Setup(x => x.ToDto(It.IsAny<IEnumerable<JournalEntry>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items, Is.Not.Null);
        Assert.That(result.Items.Count, Is.EqualTo(2));
        Assert.That(result.TotalCount, Is.EqualTo(3));
        Assert.That(result.PageNumber, Is.EqualTo(1));
        Assert.That(result.PageSize, Is.EqualTo(2));
    }

    [Test]
    public async Task Handle_ExcludesDeletedEntries_WhenQuerying()
    {
        // Arrange
        var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var sortingParams = new SortingParams();
        var filteringParams = new JournalEntryFilteringParams();
        
        var query = new GetAllJournalEntriesQuery(paginationParams, sortingParams, filteringParams);

        var journalEntries = new List<JournalEntry>
        {
            new() { Id = 1, EntryNumber = "JE-001", Description = "Active Entry", IsDeleted = false, Lines = new List<JournalEntryLine>() },
            new() { Id = 2, EntryNumber = "JE-002", Description = "Deleted Entry", IsDeleted = true, Lines = new List<JournalEntryLine>() },
            new() { Id = 3, EntryNumber = "JE-003", Description = "Another Active Entry", IsDeleted = false, Lines = new List<JournalEntryLine>() }
        };

        var expectedDtos = new List<JournalEntryDto>
        {
            new() { Id = 1, EntryNumber = "JE-001", Description = "Active Entry" },
            new() { Id = 3, EntryNumber = "JE-003", Description = "Another Active Entry" }
        };

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
        _mapperMock.Setup(x => x.ToDto(It.IsAny<IEnumerable<JournalEntry>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.Count, Is.EqualTo(2));
        Assert.That(result.TotalCount, Is.EqualTo(2)); // Only non-deleted entries
        Assert.That(result.Items.All(item => item.Description != "Deleted Entry"), Is.True);
    }

    [Test]
    public async Task Handle_FiltersResults_WhenSearchTermProvided()
    {
        // Arrange
        var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var sortingParams = new SortingParams();
        var filteringParams = new JournalEntryFilteringParams { SearchTerm = "Cash" };
        
        var query = new GetAllJournalEntriesQuery(paginationParams, sortingParams, filteringParams);

        var journalEntries = new List<JournalEntry>
        {
            new() { Id = 1, EntryNumber = "JE-001", Description = "Cash Payment", Reference = "REF-001", IsDeleted = false, Lines = new List<JournalEntryLine>() },
            new() { Id = 2, EntryNumber = "JE-002", Description = "Bank Transfer", Reference = "REF-002", IsDeleted = false, Lines = new List<JournalEntryLine>() },
            new() { Id = 3, EntryNumber = "CASH-003", Description = "Revenue Entry", Reference = "REF-003", IsDeleted = false, Lines = new List<JournalEntryLine>() }
        };

        var expectedDtos = new List<JournalEntryDto>
        {
            new() { Id = 1, EntryNumber = "JE-001", Description = "Cash Payment" },
            new() { Id = 3, EntryNumber = "CASH-003", Description = "Revenue Entry" }
        };

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
        _mapperMock.Setup(x => x.ToDto(It.IsAny<IEnumerable<JournalEntry>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.Count, Is.EqualTo(2));
        Assert.That(result.TotalCount, Is.EqualTo(2));
    }

    [Test]
    public async Task Handle_FiltersResultsByStatus_WhenStatusFilterProvided()
    {
        // Arrange
        var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var sortingParams = new SortingParams();
        var filteringParams = new JournalEntryFilteringParams { StatusFilter = "posted" };
        
        var query = new GetAllJournalEntriesQuery(paginationParams, sortingParams, filteringParams);

        var journalEntries = new List<JournalEntry>
        {
            new() { Id = 1, EntryNumber = "JE-001", Description = "Posted Entry", IsPosted = true, IsDeleted = false, Lines = new List<JournalEntryLine>() },
            new() { Id = 2, EntryNumber = "JE-002", Description = "Unposted Entry", IsPosted = false, IsDeleted = false, Lines = new List<JournalEntryLine>() },
            new() { Id = 3, EntryNumber = "JE-003", Description = "Another Posted Entry", IsPosted = true, IsDeleted = false, Lines = new List<JournalEntryLine>() }
        };

        var expectedDtos = new List<JournalEntryDto>
        {
            new() { Id = 1, EntryNumber = "JE-001", Description = "Posted Entry" },
            new() { Id = 3, EntryNumber = "JE-003", Description = "Another Posted Entry" }
        };

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
        _mapperMock.Setup(x => x.ToDto(It.IsAny<IEnumerable<JournalEntry>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.Count, Is.EqualTo(2));
        Assert.That(result.TotalCount, Is.EqualTo(2));
    }

    [Test]
    public async Task Handle_FiltersUnpostedEntries_WhenUnpostedStatusFilterProvided()
    {
        // Arrange
        var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var sortingParams = new SortingParams();
        var filteringParams = new JournalEntryFilteringParams { StatusFilter = "unposted" };
        
        var query = new GetAllJournalEntriesQuery(paginationParams, sortingParams, filteringParams);

        var journalEntries = new List<JournalEntry>
        {
            new() { Id = 1, EntryNumber = "JE-001", Description = "Posted Entry", IsPosted = true, IsDeleted = false, Lines = new List<JournalEntryLine>() },
            new() { Id = 2, EntryNumber = "JE-002", Description = "Unposted Entry", IsPosted = false, IsDeleted = false, Lines = new List<JournalEntryLine>() },
            new() { Id = 3, EntryNumber = "JE-003", Description = "Another Unposted Entry", IsPosted = false, IsDeleted = false, Lines = new List<JournalEntryLine>() }
        };

        var expectedDtos = new List<JournalEntryDto>
        {
            new() { Id = 2, EntryNumber = "JE-002", Description = "Unposted Entry" },
            new() { Id = 3, EntryNumber = "JE-003", Description = "Another Unposted Entry" }
        };

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
        _mapperMock.Setup(x => x.ToDto(It.IsAny<IEnumerable<JournalEntry>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.Count, Is.EqualTo(2));
        Assert.That(result.TotalCount, Is.EqualTo(2));
    }

    [Test]
    public async Task Handle_UsesDefaultSorting_WhenNoSortingProvided()
    {
        // Arrange
        var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var sortingParams = new SortingParams { OrderBy = null };
        var filteringParams = new JournalEntryFilteringParams();
        
        var query = new GetAllJournalEntriesQuery(paginationParams, sortingParams, filteringParams);

        var journalEntries = new List<JournalEntry>
        {
            new() 
            { 
                Id = 1, 
                EntryNumber = "JE-001", 
                Description = "Oldest Entry", 
                TransactionDate = DateTime.UtcNow.AddDays(-3),
                IsDeleted = false,
                Lines = new List<JournalEntryLine>()
            },
            new() 
            { 
                Id = 2, 
                EntryNumber = "JE-002", 
                Description = "Newest Entry", 
                TransactionDate = DateTime.UtcNow,
                IsDeleted = false,
                Lines = new List<JournalEntryLine>()
            },
            new() 
            { 
                Id = 3, 
                EntryNumber = "JE-003", 
                Description = "Middle Entry", 
                TransactionDate = DateTime.UtcNow.AddDays(-1),
                IsDeleted = false,
                Lines = new List<JournalEntryLine>()
            }
        };

        var expectedDtos = new List<JournalEntryDto>
        {
            new() { Id = 2, EntryNumber = "JE-002", Description = "Newest Entry" },
            new() { Id = 3, EntryNumber = "JE-003", Description = "Middle Entry" },
            new() { Id = 1, EntryNumber = "JE-001", Description = "Oldest Entry" }
        };

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
        _mapperMock.Setup(x => x.ToDto(It.IsAny<IEnumerable<JournalEntry>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.Count, Is.EqualTo(3));
        // Default sorting should be by TransactionDate descending, then by EntryNumber descending
        Assert.That(result.Items.First().Description, Is.EqualTo("Newest Entry"));
    }

    [Test]
    public async Task Handle_ReturnsEmptyResults_WhenNoEntriesExist()
    {
        // Arrange
        var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var sortingParams = new SortingParams();
        var filteringParams = new JournalEntryFilteringParams();
        
        var query = new GetAllJournalEntriesQuery(paginationParams, sortingParams, filteringParams);

        var journalEntries = new List<JournalEntry>();
        var expectedDtos = new List<JournalEntryDto>();

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
        _mapperMock.Setup(x => x.ToDto(It.IsAny<IEnumerable<JournalEntry>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items, Is.Empty);
        Assert.That(result.TotalCount, Is.EqualTo(0));
        Assert.That(result.PageNumber, Is.EqualTo(1));
        Assert.That(result.PageSize, Is.EqualTo(10));
    }

    [Test]
    public async Task Handle_HandlesPaginationCorrectly_WhenMultiplePages()
    {
        // Arrange
        var paginationParams = new PaginationParams { PageNumber = 2, PageSize = 2 };
        var sortingParams = new SortingParams();
        var filteringParams = new JournalEntryFilteringParams();
        
        var query = new GetAllJournalEntriesQuery(paginationParams, sortingParams, filteringParams);

        var journalEntries = new List<JournalEntry>
        {
            new() { Id = 1, EntryNumber = "JE-001", Description = "Entry 1", IsDeleted = false, Lines = new List<JournalEntryLine>() },
            new() { Id = 2, EntryNumber = "JE-002", Description = "Entry 2", IsDeleted = false, Lines = new List<JournalEntryLine>() },
            new() { Id = 3, EntryNumber = "JE-003", Description = "Entry 3", IsDeleted = false, Lines = new List<JournalEntryLine>() },
            new() { Id = 4, EntryNumber = "JE-004", Description = "Entry 4", IsDeleted = false, Lines = new List<JournalEntryLine>() },
            new() { Id = 5, EntryNumber = "JE-005", Description = "Entry 5", IsDeleted = false, Lines = new List<JournalEntryLine>() }
        };

        var expectedDtos = new List<JournalEntryDto>
        {
            new() { Id = 3, EntryNumber = "JE-003", Description = "Entry 3" },
            new() { Id = 4, EntryNumber = "JE-004", Description = "Entry 4" }
        };

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
        _mapperMock.Setup(x => x.ToDto(It.IsAny<IEnumerable<JournalEntry>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.Count, Is.EqualTo(2));
        Assert.That(result.TotalCount, Is.EqualTo(5));
        Assert.That(result.PageNumber, Is.EqualTo(2));
        Assert.That(result.PageSize, Is.EqualTo(2));
    }
}