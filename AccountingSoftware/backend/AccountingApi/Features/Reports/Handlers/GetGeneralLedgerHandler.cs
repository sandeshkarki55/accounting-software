using AccountingApi.DTOs.Reports;
using AccountingApi.Features.Reports;

using MyMediator;

namespace AccountingApi.Features.Reports.Handlers;

public class GetGeneralLedgerQuery : IRequest<GeneralLedgerDto>
{
    public int AccountId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class GetGeneralLedgerHandler(IReportService reportService) : IRequestHandler<GetGeneralLedgerQuery, GeneralLedgerDto>
{
    public async Task<GeneralLedgerDto> Handle(GetGeneralLedgerQuery request, CancellationToken cancellationToken)
        => await reportService.GetGeneralLedgerAsync(request.AccountId, request.StartDate, request.EndDate);
}
