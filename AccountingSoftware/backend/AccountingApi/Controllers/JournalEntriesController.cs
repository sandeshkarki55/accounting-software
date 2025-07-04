using Microsoft.AspNetCore.Mvc;
using MediatR;
using AccountingApi.Features.JournalEntries;
using AccountingApi.DTOs;

namespace AccountingApi.Controllers;

public class JournalEntriesController(IMediator mediator) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<JournalEntryDto>>> GetJournalEntries()
    {
        try
        {
            var journalEntries = await mediator.Send(new GetAllJournalEntriesQuery());
            return Ok(journalEntries);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving journal entries.", details = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<JournalEntryDto>> CreateJournalEntry(CreateJournalEntryDto createJournalEntryDto)
    {
        try
        {
            var journalEntry = await mediator.Send(new CreateJournalEntryCommand(createJournalEntryDto));
            return CreatedAtAction(nameof(GetJournalEntries), new { id = journalEntry.Id }, journalEntry);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the journal entry.", details = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteJournalEntry(int id)
    {
        try
        {
            var result = await mediator.Send(new DeleteJournalEntryCommand(id));
            if (!result)
                return NotFound(new { message = "Journal entry not found." });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the journal entry.", details = ex.Message });
        }
    }

    [HttpDelete("lines/{id:int}")]
    public async Task<IActionResult> DeleteJournalEntryLine(int id)
    {
        try
        {
            var result = await mediator.Send(new DeleteJournalEntryLineCommand(id));
            if (!result)
                return NotFound(new { message = "Journal entry line not found." });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the journal entry line.", details = ex.Message });
        }
    }
}
