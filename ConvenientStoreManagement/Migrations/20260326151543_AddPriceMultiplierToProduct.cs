using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConvenientStoreManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceMultiplierToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PriceMultiplier",
                table: "Products",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 1.5m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceMultiplier",
                table: "Products");
        }
    }
}
