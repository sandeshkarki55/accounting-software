using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTaxRateColumnType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TaxRate",
                table: "Invoices",
                type: "decimal(6,4)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,4)",
                oldDefaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TaxRate",
                table: "Invoices",
                type: "decimal(5,4)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,4)",
                oldDefaultValue: 0m);
        }
    }
}
