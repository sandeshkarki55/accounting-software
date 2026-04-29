using AccountingApi.DTOs.Reports;
using AccountingApi.Features.Reports;

using MyMediator;

namespace AccountingApi.Features.Reports.Handlers;

public class GetTrialBalanceQuery : IRequest<TrialBalanceDto>
{
    public DateTime? AsOfDate { get; set; }
}

public class GetTrialBalanceHandler(IReportService reportService) : IRequestHandler<GetTrialBalanceQuery, TrialBalanceDto>
{
    public async Task<TrialBalanceDto> Handle(GetTrialBalanceQuery request, CancellationToken cancellationToken)
        => await reportService.GetTrialBalanceAsync(request.AsOfDate);
}
