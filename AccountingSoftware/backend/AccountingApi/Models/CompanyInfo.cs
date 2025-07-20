namespace AccountingApi.Models;

// Company Information entity for invoice headers and company settings
public class CompanyInfo : BaseEntity
{
    public string CompanyName { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string BankAccountNumber { get; set; } = string.Empty;
    public string BankRoutingNumber { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public bool IsDefault { get; set; } = false;

    // Navigation property for invoices that use this company info
    public ICollection<Invoice> Invoices { get; set; } = [];
}