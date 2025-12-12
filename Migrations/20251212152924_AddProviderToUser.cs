using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseVista.API.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProviderKey",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderName",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProviderKey",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProviderName",
                table: "AspNetUsers");
        }
    }
}
