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
}