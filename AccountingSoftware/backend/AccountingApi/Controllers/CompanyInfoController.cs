using Microsoft.AspNetCore.Mvc;
using MediatR;
using AccountingApi.DTOs;
using AccountingApi.Features.CompanyInfo;

namespace AccountingApi.Controllers;

public class CompanyInfoController(IMediator mediator) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CompanyInfoDto>>> GetCompanyInfos()
    {
        var companyInfos = await mediator.Send(new GetAllCompanyInfosQuery());
        return Ok(companyInfos);
    }

    [HttpPost]
    public async Task<ActionResult<CompanyInfoDto>> CreateCompanyInfo(CreateCompanyInfoDto createCompanyInfoDto)
    {
        var companyInfo = await mediator.Send(new CreateCompanyInfoCommand(createCompanyInfoDto));
        return CreatedAtAction(nameof(GetCompanyInfos), new { id = companyInfo.Id }, companyInfo);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CompanyInfoDto>> UpdateCompanyInfo(int id, CreateCompanyInfoDto updateCompanyInfoDto)
    {
        var companyInfo = await mediator.Send(new UpdateCompanyInfoCommand(id, updateCompanyInfoDto));
        return Ok(companyInfo);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCompanyInfo(int id)
    {
        try
        {
            var result = await mediator.Send(new DeleteCompanyInfoCommand(id));
            if (!result)
                return NotFound(new { message = "Company info not found." });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the company info.", details = ex.Message });
        }
    }

    [HttpPut("{id}/set-default")]
    public async Task<ActionResult<CompanyInfoDto>> SetDefaultCompany(int id)
    {
        var companyInfo = await mediator.Send(new SetDefaultCompanyCommand(id));
        return Ok(companyInfo);
    }
}