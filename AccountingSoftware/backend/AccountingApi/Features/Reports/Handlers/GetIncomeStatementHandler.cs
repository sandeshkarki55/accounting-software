using AccountingApi.DTOs.Reports;
using AccountingApi.Features.Reports;

using MyMediator;

namespace AccountingApi.Features.Reports.Handlers;

public class GetIncomeStatementQuery : IRequest<IncomeStatementDto>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class GetIncomeStatementHandler(IReportService reportService) : IRequestHandler<GetIncomeStatementQuery, IncomeStatementDto>
{
    public async Task<IncomeStatementDto> Handle(GetIncomeStatementQuery request, CancellationToken cancellationToken)
        => await reportService.GetIncomeStatementAsync(request.StartDate, request.EndDate);
}
