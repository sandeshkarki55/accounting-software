using AccountingApi.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountingApi.Infrastructure.Configurations;

public class JournalEntryLineConfiguration : IEntityTypeConfiguration<JournalEntryLine>
{
    public void Configure(EntityTypeBuilder<JournalEntryLine> builder)
    {
        // Primary key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.DebitAmount)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(e => e.CreditAmount)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        // Foreign key relationships
        builder.HasOne(e => e.JournalEntry)
            .WithMany(e => e.Lines)
            .HasForeignKey(e => e.JournalEntryId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_JournalEntryLine_JournalEntry");

        builder.HasOne(e => e.Account)
            .WithMany(a => a.JournalEntryLines) // Now properly references the correct navigation property
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_JournalEntryLine_Account");

        // Indexes
        builder.HasIndex(e => e.JournalEntryId)
            .HasDatabaseName("IX_JournalEntryLine_JournalEntryId");

        builder.HasIndex(e => e.AccountId)
            .HasDatabaseName("IX_JournalEntryLine_AccountId");

        // Table configuration with check constraint
        builder.ToTable("JournalEntryLines", t =>
            t.HasCheckConstraint("CK_JournalEntryLine_DebitOrCredit",
                "(DebitAmount = 0 AND CreditAmount > 0) OR (DebitAmount > 0 AND CreditAmount = 0)")
        );
    }
}