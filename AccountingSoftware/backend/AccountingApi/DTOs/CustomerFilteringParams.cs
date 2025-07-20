namespace AccountingApi.DTOs;

public class CustomerFilteringParams
{
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    // Add more filters as needed
}