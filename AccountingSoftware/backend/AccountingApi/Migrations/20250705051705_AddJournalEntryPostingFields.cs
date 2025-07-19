using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingApi.Migrations
{
    /// <inheritdoc />
    public partial class AddJournalEntryPostingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PostedAt",
                table: "JournalEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostedBy",
                table: "JournalEntries",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PostedAt",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "PostedBy",
                table: "JournalEntries");
        }
    }
}
