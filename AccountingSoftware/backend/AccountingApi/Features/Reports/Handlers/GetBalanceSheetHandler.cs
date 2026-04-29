using AccountingApi.DTOs.Reports;
using AccountingApi.Features.Reports;

using MyMediator;

namespace AccountingApi.Features.Reports.Handlers;

public class GetBalanceSheetQuery : IRequest<BalanceSheetDto>
{
    public DateTime? AsOfDate { get; set; }
}

public class GetBalanceSheetHandler(IReportService reportService) : IRequestHandler<GetBalanceSheetQuery, BalanceSheetDto>
{
    public async Task<BalanceSheetDto> Handle(GetBalanceSheetQuery request, CancellationToken cancellationToken)
        => await reportService.GetBalanceSheetAsync(request.AsOfDate);
}
