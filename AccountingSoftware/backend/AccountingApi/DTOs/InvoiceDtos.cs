using AccountingApi.Models;

namespace AccountingApi.DTOs;

// Invoice DTOs
public class InvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int? CompanyInfoId { get; set; }
    public string? CompanyName { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public InvoiceStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string Terms { get; set; } = string.Empty;
    public DateTime? PaidDate { get; set; }
    public string? PaymentReference { get; set; }
    public List<InvoiceItemDto> Items { get; set; } = [];
}

public class InvoiceItemDto
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
    public int SortOrder { get; set; }
}

public class CreateInvoiceDto
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public int CustomerId { get; set; }
    public int? CompanyInfoId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal TaxRate { get; set; }
    public decimal DiscountAmount { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string Terms { get; set; } = string.Empty;
    public List<CreateInvoiceItemDto> Items { get; set; } = [];
}

public class CreateInvoiceItemDto
{
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateInvoiceDto
{
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public int CustomerId { get; set; }
    public int? CompanyInfoId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal TaxRate { get; set; }
    public decimal DiscountAmount { get; set; }
    public InvoiceStatus Status { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string Terms { get; set; } = string.Empty;
    public DateTime? PaidDate { get; set; }
    public string? PaymentReference { get; set; }
    public List<UpdateInvoiceItemDto> Items { get; set; } = [];
}

public class UpdateInvoiceItemDto
{
    public int? Id { get; set; } // null for new items
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int SortOrder { get; set; }
}

public class MarkInvoiceAsPaidDto
{
    public DateTime PaidDate { get; set; } = DateTime.Today;
    public string? PaymentReference { get; set; }
}