using Microsoft.AspNetCore.Mvc;
using MediatR;
using AccountingApi.DTOs;
using AccountingApi.Features.CompanyInfo;

namespace AccountingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompanyInfoController : ControllerBase
{
    private readonly IMediator _mediator;

    public CompanyInfoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CompanyInfoDto>>> GetCompanyInfos()
    {
        var companyInfos = await _mediator.Send(new GetAllCompanyInfosQuery());
        return Ok(companyInfos);
    }

    [HttpPost]
    public async Task<ActionResult<CompanyInfoDto>> CreateCompanyInfo(CreateCompanyInfoDto createCompanyInfoDto)
    {
        var companyInfo = await _mediator.Send(new CreateCompanyInfoCommand(createCompanyInfoDto));
        return CreatedAtAction(nameof(GetCompanyInfos), new { id = companyInfo.Id }, companyInfo);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CompanyInfoDto>> UpdateCompanyInfo(int id, CreateCompanyInfoDto updateCompanyInfoDto)
    {
        var companyInfo = await _mediator.Send(new UpdateCompanyInfoCommand(id, updateCompanyInfoDto));
        return Ok(companyInfo);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCompanyInfo(int id)
    {
        await _mediator.Send(new DeleteCompanyInfoCommand(id));
        return NoContent();
    }

    [HttpPut("{id}/set-default")]
    public async Task<ActionResult<CompanyInfoDto>> SetDefaultCompany(int id)
    {
        var companyInfo = await _mediator.Send(new SetDefaultCompanyCommand(id));
        return Ok(companyInfo);
    }
}