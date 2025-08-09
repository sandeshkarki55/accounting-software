using AccountingApi.Controllers;
using AccountingApi.DTOs;
using AccountingApi.Features.JournalEntries;

using Microsoft.AspNetCore.Mvc;

using Moq;

using MyMediator;

namespace AccountingApi.Tests.Controllers;

public class JournalEntriesControllerTests
{
    private Mock<IMediator> _mediatorMock = null!;
    private JournalEntriesController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
        _controller = new JournalEntriesController(_mediatorMock.Object);
    }

    [Test]
    public async Task GetJournalEntries_ReturnsOk_WithPagedResults()
    {
        // Arrange
        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var sorting = new SortingParams { SortField = "EntryDate", SortOrder = "desc" };
        var filtering = new JournalEntryFilteringParams { SearchTerm = "JE-2024" };
        
        var expectedResult = new PagedResult<JournalEntryDto>
        {
            Items = new List<JournalEntryDto>
            {
                new() { Id = 1, EntryNumber = "JE-2024-001", Description = "Opening balance" },
                new() { Id = 2, EntryNumber = "JE-2024-002", Description = "Cash payment" }
            },
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllJournalEntriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetJournalEntries(pagination, sorting, filtering);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(expectedResult));
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<GetAllJournalEntriesQuery>(q => 
                q.PaginationParams == pagination && 
                q.SortingParams == sorting && 
                q.FilteringParams == filtering), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CreateJournalEntry_ReturnsCreatedAtAction_WhenSuccessful()
    {
        // Arrange
        var createJournalEntryDto = new CreateJournalEntryDto
        {
            Description = "Test journal entry",
            EntryDate = DateTime.UtcNow,
            Lines = new List<JournalEntryLineDto>
            {
                new() { AccountId = 1, DebitAmount = 1000, CreditAmount = 0, Description = "Debit entry" },
                new() { AccountId = 2, DebitAmount = 0, CreditAmount = 1000, Description = "Credit entry" }
            }
        };

        var createdJournalEntry = new JournalEntryDto
        {
            Id = 1,
            EntryNumber = "JE-2024-001",
            Description = "Test journal entry",
            EntryDate = createJournalEntryDto.EntryDate,
            Status = JournalEntryStatus.Draft,
            Lines = createJournalEntryDto.Lines
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateJournalEntryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdJournalEntry);

        // Act
        var result = await _controller.CreateJournalEntry(createJournalEntryDto);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.That(createdResult.ActionName, Is.EqualTo(nameof(JournalEntriesController.GetJournalEntries)));
        Assert.That(createdResult.RouteValues!["id"], Is.EqualTo(createdJournalEntry.Id));
        Assert.That(createdResult.Value, Is.EqualTo(createdJournalEntry));
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<CreateJournalEntryCommand>(c => c.JournalEntry == createJournalEntryDto), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteJournalEntry_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        const int journalEntryId = 1;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteJournalEntryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteJournalEntry(journalEntryId);

        // Assert
        var noContentResult = result as NoContentResult;
        Assert.That(noContentResult, Is.Not.Null);
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<DeleteJournalEntryCommand>(c => c.Id == journalEntryId), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteJournalEntry_ReturnsNotFound_WhenJournalEntryDoesNotExist()
    {
        // Arrange
        const int journalEntryId = 99;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteJournalEntryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteJournalEntry(journalEntryId);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        
        var response = notFoundResult.Value;
        Assert.That(response, Is.Not.Null);
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<DeleteJournalEntryCommand>(c => c.Id == journalEntryId), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteJournalEntryLine_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        const int journalEntryLineId = 1;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteJournalEntryLineCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteJournalEntryLine(journalEntryLineId);

        // Assert
        var noContentResult = result as NoContentResult;
        Assert.That(noContentResult, Is.Not.Null);
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<DeleteJournalEntryLineCommand>(c => c.Id == journalEntryLineId), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteJournalEntryLine_ReturnsNotFound_WhenJournalEntryLineDoesNotExist()
    {
        // Arrange
        const int journalEntryLineId = 99;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteJournalEntryLineCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteJournalEntryLine(journalEntryLineId);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        
        var response = notFoundResult.Value;
        Assert.That(response, Is.Not.Null);
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<DeleteJournalEntryLineCommand>(c => c.Id == journalEntryLineId), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateJournalEntry_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        const int journalEntryId = 1;
        var updateJournalEntryDto = new UpdateJournalEntryDto
        {
            Description = "Updated journal entry",
            EntryDate = DateTime.UtcNow,
            Lines = new List<JournalEntryLineDto>
            {
                new() { AccountId = 1, DebitAmount = 1500, CreditAmount = 0, Description = "Updated debit entry" },
                new() { AccountId = 2, DebitAmount = 0, CreditAmount = 1500, Description = "Updated credit entry" }
            }
        };

        var updatedJournalEntry = new JournalEntryDto
        {
            Id = journalEntryId,
            EntryNumber = "JE-2024-001",
            Description = "Updated journal entry",
            EntryDate = updateJournalEntryDto.EntryDate,
            Status = JournalEntryStatus.Draft,
            Lines = updateJournalEntryDto.Lines
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateJournalEntryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedJournalEntry);

        // Act
        var result = await _controller.UpdateJournalEntry(journalEntryId, updateJournalEntryDto);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(updatedJournalEntry));
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<UpdateJournalEntryCommand>(c => c.Id == journalEntryId && c.JournalEntry == updateJournalEntryDto), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task PostJournalEntry_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        const int journalEntryId = 1;

        _mediatorMock.Setup(m => m.Send(It.IsAny<PostJournalEntryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.PostJournalEntry(journalEntryId);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        
        var response = okResult.Value;
        Assert.That(response, Is.Not.Null);
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<PostJournalEntryCommand>(c => c.Id == journalEntryId), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task PostJournalEntry_ReturnsNotFound_WhenJournalEntryDoesNotExist()
    {
        // Arrange
        const int journalEntryId = 99;

        _mediatorMock.Setup(m => m.Send(It.IsAny<PostJournalEntryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.PostJournalEntry(journalEntryId);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        
        var response = notFoundResult.Value;
        Assert.That(response, Is.Not.Null);
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<PostJournalEntryCommand>(c => c.Id == journalEntryId), 
            It.IsAny<CancellationToken>()), Times.Once);
    }
}