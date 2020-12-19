using Microsoft.EntityFrameworkCore.Migrations;

namespace ePiggyWeb.DataBase.Migrations
{
    public partial class AddCurrency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Incomes",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Goals",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Expenses",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Incomes");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Expenses");
        }
    }
}
