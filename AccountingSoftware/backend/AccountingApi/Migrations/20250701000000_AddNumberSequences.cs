using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingApi.Migrations
{
    /// <inheritdoc />
    public partial class AddNumberSequences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create sequence for invoice numbers
            migrationBuilder.Sql("CREATE SEQUENCE InvoiceSequence START WITH 1 INCREMENT BY 1");
            
            // Create sequence for customer codes
            migrationBuilder.Sql("CREATE SEQUENCE CustomerSequence START WITH 1 INCREMENT BY 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop sequences
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS InvoiceSequence");
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS CustomerSequence");
        }
    }
}
