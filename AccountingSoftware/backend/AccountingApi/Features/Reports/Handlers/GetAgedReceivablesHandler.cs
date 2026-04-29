using AccountingApi.DTOs.Reports;
using AccountingApi.Features.Reports;

using MyMediator;

namespace AccountingApi.Features.Reports.Handlers;

public class GetAgedReceivablesQuery : IRequest<AgedReceivablesDto>
{
    public DateTime? AsOfDate { get; set; }
}

public class GetAgedReceivablesHandler(IReportService reportService) : IRequestHandler<GetAgedReceivablesQuery, AgedReceivablesDto>
{
    public async Task<AgedReceivablesDto> Handle(GetAgedReceivablesQuery request, CancellationToken cancellationToken)
        => await reportService.GetAgedReceivablesAsync(request.AsOfDate);
}
