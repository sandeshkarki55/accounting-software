using AccountingApi.DTOs;
using AccountingApi.Models;

namespace AccountingApi.Mappings;

/// <summary>
/// Mapper for Invoice entity and related DTOs
/// </summary>
public class InvoiceMapper(InvoiceItemMapper itemMapper) : IEntityMapper<Invoice, InvoiceDto, CreateInvoiceDto, UpdateInvoiceDto>
{
    /// <summary>
    /// Maps an Invoice entity to InvoiceDto
    /// </summary>
    /// <param name="entity">The Invoice entity to map</param>
    /// <returns>The mapped InvoiceDto</returns>
    public InvoiceDto ToDto(Invoice entity)
    {
        return new InvoiceDto
        {
            Id = entity.Id,
            InvoiceNumber = entity.InvoiceNumber,
            InvoiceDate = entity.InvoiceDate,
            DueDate = entity.DueDate,
            CustomerId = entity.CustomerId,
            CustomerName = entity.Customer?.CompanyName ?? string.Empty,
            CompanyInfoId = entity.CompanyInfoId,
            CompanyName = entity.CompanyInfo?.CompanyName,
            Description = entity.Description,
            SubTotal = entity.SubTotal,
            TaxRate = entity.TaxRate,
            TaxAmount = entity.TaxAmount,
            DiscountAmount = entity.DiscountAmount,
            TotalAmount = entity.TotalAmount,
            Status = entity.Status,
            StatusName = entity.Status.ToString(),
            Notes = entity.Notes,
            Terms = entity.Terms,
            PaidDate = entity.PaidDate,
            PaymentReference = entity.PaymentReference,
            Items = entity.Items?.Select(itemMapper.ToDto).ToList() ?? []
        };
    }

    /// <summary>
    /// Maps a collection of Invoice entities to InvoiceDto collection
    /// </summary>
    /// <param name="entities">The Invoice entities to map</param>
    /// <returns>The mapped InvoiceDto collection</returns>
    public IEnumerable<InvoiceDto> ToDto(IEnumerable<Invoice> entities)
    {
        return entities.Select(ToDto);
    }

    /// <summary>
    /// Maps a CreateInvoiceDto to Invoice entity
    /// </summary>
    /// <param name="createDto">The CreateInvoiceDto to map</param>
    /// <returns>The mapped Invoice entity</returns>
    public Invoice ToEntity(CreateInvoiceDto createDto)
    {
        var invoice = new Invoice
        {
            // InvoiceNumber will be set by the service layer using auto-generation
            InvoiceDate = createDto.InvoiceDate,
            DueDate = createDto.DueDate,
            CustomerId = createDto.CustomerId,
            CompanyInfoId = createDto.CompanyInfoId,
            Description = createDto.Description,
            TaxRate = createDto.TaxRate,
            DiscountAmount = createDto.DiscountAmount,
            Status = InvoiceStatus.Draft,
            Notes = createDto.Notes,
            Terms = createDto.Terms,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Map the items
        invoice.Items = createDto.Items?.Select(itemDto => itemMapper.ToEntity(itemDto, invoice.Id)).ToList() ?? [];

        // Calculate amounts
        CalculateInvoiceAmounts(invoice);

        return invoice;
    }

    /// <summary>
    /// Updates an existing Invoice entity with data from UpdateInvoiceDto
    /// </summary>
    /// <param name="entity">The Invoice entity to update</param>
    /// <param name="updateDto">The UpdateInvoiceDto containing new data</param>
    public void UpdateEntity(Invoice entity, UpdateInvoiceDto updateDto)
    {
        entity.InvoiceDate = updateDto.InvoiceDate;
        entity.DueDate = updateDto.DueDate;
        entity.CustomerId = updateDto.CustomerId;
        entity.CompanyInfoId = updateDto.CompanyInfoId;
        entity.Description = updateDto.Description;
        entity.TaxRate = updateDto.TaxRate;
        entity.DiscountAmount = updateDto.DiscountAmount;
        entity.Status = updateDto.Status;
        entity.Notes = updateDto.Notes;
        entity.Terms = updateDto.Terms;
        entity.PaidDate = updateDto.PaidDate;
        entity.PaymentReference = updateDto.PaymentReference;
        entity.UpdatedAt = DateTime.UtcNow;

        // Update items - this is a simplified approach, in practice you might want more sophisticated item management
        entity.Items.Clear();
        entity.Items = updateDto.Items?.Select(itemDto => itemMapper.ToEntityFromUpdate(itemDto, entity.Id)).ToList() ?? [];

        // Recalculate amounts
        CalculateInvoiceAmounts(entity);
    }

    /// <summary>
    /// Calculates the invoice amounts based on items
    /// </summary>
    /// <param name="invoice">The invoice to calculate amounts for</param>
    private static void CalculateInvoiceAmounts(Invoice invoice)
    {
        invoice.SubTotal = invoice.Items?.Sum(i => i.Amount) ?? 0;
        invoice.TaxAmount = invoice.SubTotal * (invoice.TaxRate / 100);
        invoice.TotalAmount = invoice.SubTotal + invoice.TaxAmount - invoice.DiscountAmount;
    }
}

/// <summary>
/// Mapper for InvoiceItem entity and related DTOs
/// </summary>
public class InvoiceItemMapper
{
    /// <summary>
    /// Maps an InvoiceItem entity to InvoiceItemDto
    /// </summary>
    /// <param name="entity">The InvoiceItem entity to map</param>
    /// <returns>The mapped InvoiceItemDto</returns>
    public InvoiceItemDto ToDto(InvoiceItem entity)
    {
        return new InvoiceItemDto
        {
            Id = entity.Id,
            InvoiceId = entity.InvoiceId,
            Description = entity.Description,
            Quantity = entity.Quantity,
            UnitPrice = entity.UnitPrice,
            Amount = entity.Amount,
            SortOrder = entity.SortOrder
        };
    }

    /// <summary>
    /// Maps a CreateInvoiceItemDto to InvoiceItem entity
    /// </summary>
    /// <param name="createDto">The CreateInvoiceItemDto to map</param>
    /// <param name="invoiceId">The ID of the parent invoice</param>
    /// <returns>The mapped InvoiceItem entity</returns>
    public InvoiceItem ToEntity(CreateInvoiceItemDto createDto, int invoiceId)
    {
        var item = new InvoiceItem
        {
            InvoiceId = invoiceId,
            Description = createDto.Description,
            Quantity = createDto.Quantity,
            UnitPrice = createDto.UnitPrice,
            SortOrder = createDto.SortOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Calculate amount
        item.Amount = item.Quantity * item.UnitPrice;

        return item;
    }

    /// <summary>
    /// Maps an UpdateInvoiceItemDto to InvoiceItem entity
    /// </summary>
    /// <param name="updateDto">The UpdateInvoiceItemDto to map</param>
    /// <param name="invoiceId">The ID of the parent invoice</param>
    /// <returns>The mapped InvoiceItem entity</returns>
    public InvoiceItem ToEntityFromUpdate(UpdateInvoiceItemDto updateDto, int invoiceId)
    {
        var item = new InvoiceItem
        {
            Id = updateDto.Id ?? 0, // 0 for new items
            InvoiceId = invoiceId,
            Description = updateDto.Description,
            Quantity = updateDto.Quantity,
            UnitPrice = updateDto.UnitPrice,
            SortOrder = updateDto.SortOrder,
            UpdatedAt = DateTime.UtcNow
        };

        // Calculate amount
        item.Amount = item.Quantity * item.UnitPrice;

        // Set created date for new items
        if (item.Id == 0)
        {
            item.CreatedAt = DateTime.UtcNow;
        }

        return item;
    }
}