using AccountingApi.Models;

namespace AccountingApi.Services.AutomaticJournalEntryService;

/// <summary>
/// Service for automatically creating journal entries from business transactions
/// </summary>
public interface IAutomaticJournalEntryService
{
    /// <summary>
    /// Creates journal entry when an invoice is created (A/R and Revenue)
    /// </summary>
    /// <param name="invoice">The created invoice</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created journal entry</returns>
    Task<JournalEntry> CreateInvoiceJournalEntryAsync(Invoice invoice, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates journal entry when an invoice is paid (Cash and A/R)
    /// </summary>
    /// <param name="invoice">The paid invoice</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created journal entry</returns>
    Task<JournalEntry> CreatePaymentJournalEntryAsync(Invoice invoice, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reverses journal entries when an invoice is cancelled
    /// </summary>
    /// <param name="invoice">The cancelled invoice</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The reversal journal entry</returns>
    Task<JournalEntry?> CreateCancellationJournalEntryAsync(Invoice invoice, CancellationToken cancellationToken = default);
}