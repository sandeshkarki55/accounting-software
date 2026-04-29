using AccountingApi.DTOs.Reports;

namespace AccountingApi.Features.Reports
{
    public interface IReportService
    {
        Task<TrialBalanceDto> GetTrialBalanceAsync(DateTime? asOfDate);
        Task<IncomeStatementDto> GetIncomeStatementAsync(DateTime startDate, DateTime endDate);
        Task<BalanceSheetDto> GetBalanceSheetAsync(DateTime? asOfDate);
        Task<GeneralLedgerDto> GetGeneralLedgerAsync(int accountId, DateTime startDate, DateTime endDate);
        Task<AgedReceivablesDto> GetAgedReceivablesAsync(DateTime? asOfDate);
    }
}
