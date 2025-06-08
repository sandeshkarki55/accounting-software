using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AccountingApi.Models;

namespace AccountingApi.Infrastructure.Configurations;

public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        // Primary key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.EntryNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Reference)
            .HasMaxLength(100);

        builder.Property(e => e.TotalAmount)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(e => e.IsPosted)
            .HasDefaultValue(false);

        builder.Property(e => e.TransactionDate)
            .IsRequired();

        // Indexes
        builder.HasIndex(e => e.EntryNumber)
            .IsUnique()
            .HasDatabaseName("IX_JournalEntry_EntryNumber");

        builder.HasIndex(e => e.TransactionDate)
            .HasDatabaseName("IX_JournalEntry_TransactionDate");

        builder.HasIndex(e => e.IsPosted)
            .HasDatabaseName("IX_JournalEntry_IsPosted");

        // Table name
        builder.ToTable("JournalEntries");
    }
}