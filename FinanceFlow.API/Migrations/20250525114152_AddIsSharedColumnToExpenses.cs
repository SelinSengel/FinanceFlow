using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceFlow.API.Migrations
{
    /// <inheritdoc />
    public partial class AddIsSharedColumnToExpenses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsShared",
                table: "Expenses",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsShared",
                table: "Expenses");
        }
    }
}
