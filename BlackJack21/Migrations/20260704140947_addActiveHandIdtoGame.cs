using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlackJack21.Migrations
{
    /// <inheritdoc />
    public partial class addActiveHandIdtoGame : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActiveHandId",
                table: "Games",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveHandId",
                table: "Games");
        }
    }
}
