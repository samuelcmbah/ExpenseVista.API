using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseVista.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Budgets_ApplicationUserId",
                table: "Budgets");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_ApplicationUserId_BudgetMonth",
                table: "Budgets",
                columns: new[] { "ApplicationUserId", "BudgetMonth" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Budgets_ApplicationUserId_BudgetMonth",
                table: "Budgets");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_ApplicationUserId",
                table: "Budgets",
                column: "ApplicationUserId");
        }
    }
}
