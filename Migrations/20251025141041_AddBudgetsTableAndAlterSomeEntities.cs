using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseVista.API.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetsTableAndAlterSomeEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropTable(
                name: "Users");



            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Budgets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MonthlyLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BudgetMonth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Budgets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Budgets_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });



            migrationBuilder.CreateIndex(
                name: "IX_Budgets_ApplicationUserId",
                table: "Budgets",
                column: "ApplicationUserId");


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.DropTable(
                name: "Budgets");



            migrationBuilder.DropColumn(
                name: "Type",
                table: "Transactions");



            migrationBuilder.AddColumn<string>(
                name: "CategoryType",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });


        }
    }
}
