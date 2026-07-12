using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlackJack21.Migrations
{
    /// <inheritdoc />
    public partial class AddStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalMoneyLost",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalMoneyWon",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalMoneyLost",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TotalMoneyWon",
                table: "AspNetUsers");
        }
    }
}
