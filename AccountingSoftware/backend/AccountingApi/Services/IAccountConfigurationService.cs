using AccountingApi.Models;

namespace AccountingApi.Services;

/// <summary>
/// Configuration service for mapping business transactions to chart of accounts
/// </summary>
public interface IAccountConfigurationService
{
    /// <summary>
    /// Gets the account code for cash/bank deposits
    /// </summary>
    string GetCashAccountCode();

    /// <summary>
    /// Gets the account code for accounts receivable
    /// </summary>
    string GetAccountsReceivableCode();

    /// <summary>
    /// Gets the account code for primary revenue
    /// </summary>
    string GetRevenueAccountCode();

    /// <summary>
    /// Gets the account code for sales tax payable
    /// </summary>
    string GetSalesTaxPayableCode();

    /// <summary>
    /// Gets the account code for accounts payable
    /// </summary>
    string GetAccountsPayableCode();

    /// <summary>
    /// Gets the account code for inventory
    /// </summary>
    string GetInventoryAccountCode();

    /// <summary>
    /// Gets the account code for cost of goods sold
    /// </summary>
    string GetCostOfGoodsSoldCode();
}
