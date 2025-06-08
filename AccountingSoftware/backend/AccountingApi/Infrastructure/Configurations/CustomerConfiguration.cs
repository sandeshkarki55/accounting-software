using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AccountingApi.Models;

namespace AccountingApi.Infrastructure.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        // Primary key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.CustomerCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.ContactPersonName)
            .HasMaxLength(100);

        builder.Property(e => e.Email)
            .HasMaxLength(100);

        builder.Property(e => e.Phone)
            .HasMaxLength(20);

        builder.Property(e => e.Address)
            .HasMaxLength(500);

        builder.Property(e => e.City)
            .HasMaxLength(100);

        builder.Property(e => e.State)
            .HasMaxLength(100);

        builder.Property(e => e.PostalCode)
            .HasMaxLength(20);

        builder.Property(e => e.Country)
            .HasMaxLength(100);

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(e => e.CustomerCode)
            .IsUnique()
            .HasDatabaseName("IX_Customer_CustomerCode");

        builder.HasIndex(e => e.CompanyName)
            .HasDatabaseName("IX_Customer_CompanyName");

        builder.HasIndex(e => e.Email)
            .HasDatabaseName("IX_Customer_Email");

        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("IX_Customer_IsActive");

        // Table name
        builder.ToTable("Customers");
    }
}