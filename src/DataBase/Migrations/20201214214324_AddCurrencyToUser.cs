using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ePiggyWeb.Migrations
{
    public partial class AddCurrencyToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Users",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);
        }
    }
}
