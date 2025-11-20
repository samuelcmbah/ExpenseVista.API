using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseVista.API.Migrations
{
    /// <inheritdoc />
    public partial class IsAutomaticColumnTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAutomatic",
                table: "Transactions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAutomatic",
                table: "Transactions");
        }
    }
}
