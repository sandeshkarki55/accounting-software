using AccountingApi.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountingApi.Infrastructure.Configurations;

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        // Primary key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Quantity)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(e => e.UnitPrice)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(e => e.SortOrder)
            .HasDefaultValue(0);

        // Foreign key relationships
        builder.HasOne(e => e.Invoice)
            .WithMany(i => i.Items)
            .HasForeignKey(e => e.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_InvoiceItem_Invoice");

        // Indexes
        builder.HasIndex(e => e.InvoiceId)
            .HasDatabaseName("IX_InvoiceItem_InvoiceId");

        builder.HasIndex(e => new { e.InvoiceId, e.SortOrder })
            .HasDatabaseName("IX_InvoiceItem_InvoiceId_SortOrder");

        // Table name
        builder.ToTable("InvoiceItems");
    }
}