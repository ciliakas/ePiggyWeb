using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ePiggyWeb.DataBase.Migrations
{ 
    public partial class test3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(nullable: false),
                    Amount = table.Column<double>(nullable: false),
                    Title = table.Column<string>(maxLength: 255, nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    IsMonthly = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Incomes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(nullable: false),
                    Amount = table.Column<double>(nullable: false),
                    Title = table.Column<string>(maxLength: 255, nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    IsMonthly = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incomes", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "Incomes");
        }
    }
}
