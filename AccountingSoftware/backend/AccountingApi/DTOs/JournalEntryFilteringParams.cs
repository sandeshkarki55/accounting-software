namespace AccountingApi.DTOs
{
    public class JournalEntryFilteringParams
    {
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; } // e.g., "all", "posted", "unposted"
        // Add more journal entry-specific filters as needed
    }
}
