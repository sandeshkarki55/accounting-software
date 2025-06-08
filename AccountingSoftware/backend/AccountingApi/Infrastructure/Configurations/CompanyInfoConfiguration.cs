using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AccountingApi.Models;

namespace AccountingApi.Infrastructure.Configurations;

public class CompanyInfoConfiguration : IEntityTypeConfiguration<CompanyInfo>
{
    public void Configure(EntityTypeBuilder<CompanyInfo> builder)
    {
        // Primary key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.LegalName)
            .HasMaxLength(200);

        builder.Property(e => e.TaxNumber)
            .HasMaxLength(50);

        builder.Property(e => e.RegistrationNumber)
            .HasMaxLength(50);

        builder.Property(e => e.Email)
            .HasMaxLength(100);

        builder.Property(e => e.Phone)
            .HasMaxLength(20);

        builder.Property(e => e.Website)
            .HasMaxLength(200);

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

        builder.Property(e => e.LogoUrl)
            .HasMaxLength(500);

        builder.Property(e => e.BankName)
            .HasMaxLength(200);

        builder.Property(e => e.BankAccountNumber)
            .HasMaxLength(50);

        builder.Property(e => e.BankRoutingNumber)
            .HasMaxLength(50);

        builder.Property(e => e.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("USD");

        builder.Property(e => e.IsDefault)
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(e => e.CompanyName)
            .HasDatabaseName("IX_CompanyInfo_CompanyName");

        builder.HasIndex(e => e.IsDefault)
            .HasDatabaseName("IX_CompanyInfo_IsDefault");

        builder.HasIndex(e => e.TaxNumber)
            .HasDatabaseName("IX_CompanyInfo_TaxNumber");

        // Table name
        builder.ToTable("CompanyInfos");
    }
}