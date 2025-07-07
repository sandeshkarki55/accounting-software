namespace AccountingApi.DTOs
{
    public class InvoiceFilteringParams
    {
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; } // e.g., "all", "paid", "unpaid", "overdue", "draft", "cancelled"
        // Add more invoice-specific filters as needed
    }
}
