using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AccountingApi.Models;

namespace AccountingApi.Infrastructure.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        // Primary key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.InvoiceDate)
            .IsRequired();

        builder.Property(e => e.DueDate)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.SubTotal)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(e => e.TaxRate)
            .HasColumnType("decimal(5,4)")
            .HasDefaultValue(0);

        builder.Property(e => e.TaxAmount)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(e => e.DiscountAmount)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(e => e.TotalAmount)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(e => e.Status)
            .HasDefaultValue(InvoiceStatus.Draft);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.Property(e => e.Terms)
            .HasMaxLength(1000);

        builder.Property(e => e.PaymentReference)
            .HasMaxLength(100);

        // Foreign key relationships
        builder.HasOne(e => e.Customer)
            .WithMany(c => c.Invoices)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Invoice_Customer");

        builder.HasOne(e => e.CompanyInfo)
            .WithMany(c => c.Invoices)
            .HasForeignKey(e => e.CompanyInfoId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_Invoice_CompanyInfo");

        // Indexes
        builder.HasIndex(e => e.InvoiceNumber)
            .IsUnique()
            .HasDatabaseName("IX_Invoice_InvoiceNumber");

        builder.HasIndex(e => e.InvoiceDate)
            .HasDatabaseName("IX_Invoice_InvoiceDate");

        builder.HasIndex(e => e.DueDate)
            .HasDatabaseName("IX_Invoice_DueDate");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_Invoice_Status");

        builder.HasIndex(e => e.CustomerId)
            .HasDatabaseName("IX_Invoice_CustomerId");

        // Table name
        builder.ToTable("Invoices");
    }
}