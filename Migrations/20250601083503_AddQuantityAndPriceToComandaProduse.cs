using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace magazinmercerie.Migrations
{
    /// <inheritdoc />
    public partial class AddQuantityAndPriceToComandaProduse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add the missing quantity column
            migrationBuilder.AddColumn<decimal>(
                name: "CantitateComanda",
                table: "ComandaProduse",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            // Add the missing price column
            migrationBuilder.AddColumn<decimal>(
                name: "PretLaComanda",
                table: "ComandaProduse",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the quantity column
            migrationBuilder.DropColumn(
                name: "CantitateComanda",
                table: "ComandaProduse");

            // Remove the price column
            migrationBuilder.DropColumn(
                name: "PretLaComanda",
                table: "ComandaProduse");
        }
    }
}
