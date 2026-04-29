namespace AccountingApi.DTOs.Reports;

public record ReportFilterParams
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? AsOfDate { get; set; }
    public int? AccountId { get; set; }
}

public record TrialBalanceDto
{
    public List<TrialBalanceLineDto> Lines { get; set; } = [];
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public bool IsBalanced => Math.Abs(TotalDebits - TotalCredits) < 0.01m;
}

public record TrialBalanceLineDto
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public decimal DebitBalance { get; set; }
    public decimal CreditBalance { get; set; }
}

public record IncomeStatementDto
{
    public List<IncomeStatementSectionDto> Revenue { get; set; } = [];
    public decimal TotalRevenue { get; set; }
    public List<IncomeStatementSectionDto> CostOfGoodsSold { get; set; } = [];
    public decimal TotalCOGS { get; set; }
    public decimal GrossProfit => TotalRevenue - TotalCOGS;
    public List<IncomeStatementSectionDto> OperatingExpenses { get; set; } = [];
    public decimal TotalOperatingExpenses { get; set; }
    public decimal OperatingIncome => GrossProfit - TotalOperatingExpenses;
    public List<IncomeStatementSectionDto> OtherIncome { get; set; } = [];
    public decimal TotalOtherIncome { get; set; }
    public List<IncomeStatementSectionDto> OtherExpenses { get; set; } = [];
    public decimal TotalOtherExpenses { get; set; }
    public decimal NetIncome => OperatingIncome + TotalOtherIncome - TotalOtherExpenses;
}

public record IncomeStatementSectionDto
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public record BalanceSheetDto
{
    public List<BalanceSheetSectionDto> Assets { get; set; } = [];
    public decimal TotalAssets { get; set; }
    public List<BalanceSheetSectionDto> Liabilities { get; set; } = [];
    public decimal TotalLiabilities { get; set; }
    public List<BalanceSheetSectionDto> Equity { get; set; } = [];
    public decimal TotalEquity { get; set; }
    public decimal TotalLiabilitiesAndEquity => TotalLiabilities + TotalEquity;
    public bool IsBalanced => Math.Abs(TotalAssets - TotalLiabilitiesAndEquity) < 0.01m;
}

public record BalanceSheetSectionDto
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}

public record GeneralLedgerDto
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public List<GeneralLedgerLineDto> Lines { get; set; } = [];
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
}

public record GeneralLedgerLineDto
{
    public DateTime TransactionDate { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal RunningBalance { get; set; }
}

public record AgedReceivablesDto
{
    public List<AgedReceivablesCustomerDto> Customers { get; set; } = [];
    public AgedReceivablesSummaryDto Summary { get; set; } = new();
}

public record AgedReceivablesCustomerDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public List<AgedReceivableInvoiceDto> Invoices { get; set; } = [];
    public decimal CurrentAmount { get; set; }
    public decimal Days1To30Amount { get; set; }
    public decimal Days31To60Amount { get; set; }
    public decimal Days61To90Amount { get; set; }
    public decimal DaysOver90Amount { get; set; }
    public decimal TotalOutstanding => CurrentAmount + Days1To30Amount + Days31To60Amount + Days61To90Amount + DaysOver90Amount;
}

public record AgedReceivableInvoiceDto
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public int DaysOverdue { get; set; }
}

public record AgedReceivablesSummaryDto
{
    public decimal TotalCurrent { get; set; }
    public decimal TotalDays1To30 { get; set; }
    public decimal TotalDays31To60 { get; set; }
    public decimal TotalDays61To90 { get; set; }
    public decimal TotalDaysOver90 { get; set; }
    public decimal GrandTotal => TotalCurrent + TotalDays1To30 + TotalDays31To60 + TotalDays61To90 + TotalDaysOver90;
}
