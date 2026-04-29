using AccountingApi.DTOs;
using AccountingApi.Models;

namespace AccountingApi.Features.JournalEntries;

/// <summary>
/// Shared validation logic for journal entries.
/// </summary>
public static class JournalEntryValidationHelper
{
    /// <summary>
    /// Validates that the journal entry lines are balanced (debits == credits).
    /// </summary>
    public static void ValidateBalanced(IEnumerable<CreateJournalEntryLineDto> lines)
    {
        var totalDebits = lines.Sum(l => l.DebitAmount);
        var totalCredits = lines.Sum(l => l.CreditAmount);

        if (Math.Abs(totalDebits - totalCredits) > 0.01m) // Allow for small rounding differences
        {
            throw new InvalidOperationException(
                $"Journal entry is not balanced. Debits: {totalDebits:C}, Credits: {totalCredits:C}");
        }
    }

    /// <summary>
    /// Validates that the journal entry lines are balanced (debits == credits).
    /// </summary>
    public static void ValidateBalanced(IEnumerable<JournalEntryLine> lines)
    {
        var totalDebits = lines.Sum(l => l.DebitAmount);
        var totalCredits = lines.Sum(l => l.CreditAmount);

        if (Math.Abs(totalDebits - totalCredits) > 0.01m) // Allow for small rounding differences
        {
            throw new InvalidOperationException(
                $"Journal entry is not balanced. Debits: {totalDebits:C}, Credits: {totalCredits:C}");
        }
    }

    /// <summary>
    /// Validates that each line has exactly one of debit or credit amount (not both, not neither).
    /// </summary>
    public static void ValidateSingleEntryPerLine(IEnumerable<CreateJournalEntryLineDto> lines)
    {
        foreach (var line in lines)
        {
            if ((line.DebitAmount > 0 && line.CreditAmount > 0) ||
                (line.DebitAmount == 0 && line.CreditAmount == 0))
            {
                throw new InvalidOperationException(
                    "Each journal entry line must have either a debit amount or credit amount (but not both or neither).");
            }
        }
    }

    /// <summary>
    /// Validates that each line has exactly one of debit or credit amount (not both, not neither).
    /// </summary>
    public static void ValidateSingleEntryPerLine(IEnumerable<JournalEntryLine> lines)
    {
        foreach (var line in lines)
        {
            if ((line.DebitAmount > 0 && line.CreditAmount > 0) ||
                (line.DebitAmount == 0 && line.CreditAmount == 0))
            {
                throw new InvalidOperationException(
                    "Each journal entry line must have either a debit amount or credit amount (but not both or neither).");
            }
        }
    }
}
