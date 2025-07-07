namespace AccountingApi.DTOs;

public class AccountFilteringParams
{
    public string? SearchTerm { get; set; }
    public int? AccountType { get; set; }
    public bool? IsActive { get; set; }
    // Add more filters as needed
}
