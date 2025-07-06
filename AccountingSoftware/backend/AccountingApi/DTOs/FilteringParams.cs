namespace AccountingApi.DTOs
{
    public class FilteringParams
    {
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; } // e.g., "all", "posted", "unposted"
    }
}
