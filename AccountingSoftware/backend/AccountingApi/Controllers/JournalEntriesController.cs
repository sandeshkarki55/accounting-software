using AccountingApi.DTOs;
using AccountingApi.Features.JournalEntries;

using Microsoft.AspNetCore.Mvc;

using MyMediator;

namespace AccountingApi.Controllers;

public class JournalEntriesController(IMediator mediator) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<JournalEntryDto>>> GetJournalEntries(
        [FromQuery] PaginationParams paginationParams,
        [FromQuery] SortingParams sortingParams,
        [FromQuery] JournalEntryFilteringParams filteringParams)
    {
        var query = new GetAllJournalEntriesQuery(paginationParams, sortingParams, filteringParams);
        var pagedJournalEntries = await mediator.Send(query);
        return Ok(pagedJournalEntries);
    }

    [HttpPost]
    public async Task<ActionResult<JournalEntryDto>> CreateJournalEntry(CreateJournalEntryDto createJournalEntryDto)
    {
        var journalEntry = await mediator.Send(new CreateJournalEntryCommand(createJournalEntryDto));
        return CreatedAtAction(nameof(GetJournalEntries), new { id = journalEntry.Id }, journalEntry);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteJournalEntry(int id)
    {
        var result = await mediator.Send(new DeleteJournalEntryCommand(id));
        if (!result)
            return NotFound(new { message = "Journal entry not found." });

        return NoContent();
    }

    [HttpDelete("lines/{id:int}")]
    public async Task<IActionResult> DeleteJournalEntryLine(int id)
    {
        var result = await mediator.Send(new DeleteJournalEntryLineCommand(id));
        if (!result)
            return NotFound(new { message = "Journal entry line not found." });

        return NoContent();
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<JournalEntryDto>> UpdateJournalEntry(int id, UpdateJournalEntryDto updateJournalEntryDto)
    {
        var journalEntry = await mediator.Send(new UpdateJournalEntryCommand(id, updateJournalEntryDto));
        return Ok(journalEntry);
    }

    [HttpPost("{id:int}/post")]
    public async Task<IActionResult> PostJournalEntry(int id)
    {
        var result = await mediator.Send(new PostJournalEntryCommand(id));
        if (!result)
            return NotFound(new { message = "Journal entry not found." });

        return Ok(new { message = "Journal entry posted successfully." });
    }
}