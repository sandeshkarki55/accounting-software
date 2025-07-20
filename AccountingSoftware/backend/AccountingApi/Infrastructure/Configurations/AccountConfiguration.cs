using AccountingApi.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountingApi.Infrastructure.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        // Primary key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.AccountCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.AccountName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Balance)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(e => e.AccountCode)
            .IsUnique()
            .HasDatabaseName("IX_Account_AccountCode");

        builder.HasIndex(e => e.AccountType)
            .HasDatabaseName("IX_Account_AccountType");

        // Self-referencing relationship for parent-child accounts
        builder.HasOne(e => e.ParentAccount)
            .WithMany(e => e.SubAccounts)
            .HasForeignKey(e => e.ParentAccountId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Account_ParentAccount");

        // Table name
        builder.ToTable("Accounts");
    }
}