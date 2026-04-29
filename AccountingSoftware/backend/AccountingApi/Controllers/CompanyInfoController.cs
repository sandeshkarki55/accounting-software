using AccountingApi.Constants;
using AccountingApi.DTOs;
using AccountingApi.Features.CompanyInfo;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MyMediator;

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

    /// <summary>
    /// Create company info. Requires Admin role.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<CompanyInfoDto>> CreateCompanyInfo(CreateCompanyInfoDto createCompanyInfoDto)
    {
        var companyInfo = await mediator.Send(new CreateCompanyInfoCommand(createCompanyInfoDto));
        return CreatedAtAction(nameof(GetCompanyInfos), new { id = companyInfo.Id }, companyInfo);
    }

    /// <summary>
    /// Update company info. Requires Admin role.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<CompanyInfoDto>> UpdateCompanyInfo(int id, CreateCompanyInfoDto updateCompanyInfoDto)
    {
        var companyInfo = await mediator.Send(new UpdateCompanyInfoCommand(id, updateCompanyInfoDto));
        return Ok(companyInfo);
    }

    /// <summary>
    /// Delete company info. Requires Admin role.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteCompanyInfo(int id)
    {
        var result = await mediator.Send(new DeleteCompanyInfoCommand(id));
        if (!result)
            return NotFound(new { message = "Company info not found." });

        return NoContent();
    }

    /// <summary>
    /// Set default company. Requires Admin role.
    /// </summary>
    [HttpPut("{id}/set-default")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<CompanyInfoDto>> SetDefaultCompany(int id)
    {
        var companyInfo = await mediator.Send(new SetDefaultCompanyCommand(id));
        return Ok(companyInfo);
    }
}