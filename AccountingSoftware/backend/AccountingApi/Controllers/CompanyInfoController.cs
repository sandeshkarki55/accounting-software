using Microsoft.AspNetCore.Mvc;
using MediatR;
using AccountingApi.DTOs;
using AccountingApi.Features.CompanyInfo;

namespace AccountingApi.Controllers;

public class CompanyInfoController(IMediator mediator) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<CompanyInfoDto>>> GetCompanyInfos(
        [FromQuery] PaginationParams pagination,
        [FromQuery] SortingParams sorting,
        [FromQuery] CompanyInfoFilteringParams filtering)
    {
        var result = await mediator.Send(new GetAllCompanyInfosQuery(pagination, sorting, filtering));
        return Ok(result);
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
        var result = await mediator.Send(new DeleteCompanyInfoCommand(id));
        if (!result)
            return NotFound(new { message = "Company info not found." });

        return NoContent();
    }

    [HttpPut("{id}/set-default")]
    public async Task<ActionResult<CompanyInfoDto>> SetDefaultCompany(int id)
    {
        var companyInfo = await mediator.Send(new SetDefaultCompanyCommand(id));
        return Ok(companyInfo);
    }
}