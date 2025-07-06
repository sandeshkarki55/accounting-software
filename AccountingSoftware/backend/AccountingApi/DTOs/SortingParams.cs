namespace AccountingApi.DTOs
{
    public class SortingParams
    {
        public string? OrderBy { get; set; }
        public bool Descending { get; set; } = false;
    }
}
