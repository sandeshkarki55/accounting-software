namespace AccountingApi.Models;

// Journal entry for double-entry bookkeeping
public class JournalEntry : BaseEntity
{
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public bool IsPosted { get; set; } = false;
    public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
}

// Journal entry line items
public class JournalEntryLine : BaseEntity
{
    public int JournalEntryId { get; set; }
    public JournalEntry JournalEntry { get; set; } = null!;
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public string Description { get; set; } = string.Empty;
}