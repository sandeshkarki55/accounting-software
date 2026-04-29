using AccountingApi.DTOs.Reports;
using AccountingApi.Features.Reports.Handlers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using MyMediator;

namespace AccountingApi.Controllers;

[Authorize]
public class ReportsController(IMediator mediator) : BaseController
{
    [HttpGet("trial-balance")]
    public async Task<ActionResult<TrialBalanceDto>> GetTrialBalance([FromQuery] DateTime? asOfDate)
    {
        var result = await mediator.Send(new GetTrialBalanceQuery { AsOfDate = asOfDate });
        return Ok(result);
    }

    [HttpGet("income-statement")]
    public async Task<ActionResult<IncomeStatementDto>> GetIncomeStatement([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await mediator.Send(new GetIncomeStatementQuery { StartDate = startDate, EndDate = endDate });
        return Ok(result);
    }

    [HttpGet("balance-sheet")]
    public async Task<ActionResult<BalanceSheetDto>> GetBalanceSheet([FromQuery] DateTime? asOfDate)
    {
        var result = await mediator.Send(new GetBalanceSheetQuery { AsOfDate = asOfDate });
        return Ok(result);
    }

    [HttpGet("general-ledger")]
    public async Task<ActionResult<GeneralLedgerDto>> GetGeneralLedger([FromQuery] int accountId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await mediator.Send(new GetGeneralLedgerQuery { AccountId = accountId, StartDate = startDate, EndDate = endDate });
        return Ok(result);
    }

    [HttpGet("aged-receivables")]
    public async Task<ActionResult<AgedReceivablesDto>> GetAgedReceivables([FromQuery] DateTime? asOfDate)
    {
        var result = await mediator.Send(new GetAgedReceivablesQuery { AsOfDate = asOfDate });
        return Ok(result);
    }
}
