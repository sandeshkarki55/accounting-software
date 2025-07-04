using Microsoft.Extensions.Options;

namespace AccountingApi.Services;

/// <summary>
/// Configuration options for account mappings
/// </summary>
public class AccountMappingOptions
{
    public const string SectionName = "AccountMapping";

    public string CashAccount { get; set; } = "1000";
    public string AccountsReceivable { get; set; } = "1200";
    public string Revenue { get; set; } = "4000";
    public string SalesTaxPayable { get; set; } = "2300";
    public string AccountsPayable { get; set; } = "2000";
    public string Inventory { get; set; } = "1300";
    public string CostOfGoodsSold { get; set; } = "5000";
}

/// <summary>
/// Implementation of account configuration service
/// </summary>
public class AccountConfigurationService : IAccountConfigurationService
{
    private readonly AccountMappingOptions _options;

    public AccountConfigurationService(IOptions<AccountMappingOptions> options)
    {
        _options = options.Value;
    }

    public string GetCashAccountCode() => _options.CashAccount;

    public string GetAccountsReceivableCode() => _options.AccountsReceivable;

    public string GetRevenueAccountCode() => _options.Revenue;

    public string GetSalesTaxPayableCode() => _options.SalesTaxPayable;

    public string GetAccountsPayableCode() => _options.AccountsPayable;

    public string GetInventoryAccountCode() => _options.Inventory;

    public string GetCostOfGoodsSoldCode() => _options.CostOfGoodsSold;
}
