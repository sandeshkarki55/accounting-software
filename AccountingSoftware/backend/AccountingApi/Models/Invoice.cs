namespace AccountingApi.Models;

// Invoice entity for managing customer invoices
public class Invoice : BaseEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public int? CompanyInfoId { get; set; }
    public CompanyInfo? CompanyInfo { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public string Notes { get; set; } = string.Empty;
    public string Terms { get; set; } = string.Empty;
    public DateTime? PaidDate { get; set; }
    public string? PaymentReference { get; set; }

    // Navigation property for invoice items
    public ICollection<InvoiceItem> Items { get; set; } = [];
}

// Invoice item entity for line items on invoices
public class InvoiceItem : BaseEntity
{
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
    public int SortOrder { get; set; }
}

// Invoice status enum
public enum InvoiceStatus
{
    Draft = 0,
    Sent = 1,
    Paid = 2,
    Overdue = 3,
    Cancelled = 4,
    PartiallyPaid = 5
}