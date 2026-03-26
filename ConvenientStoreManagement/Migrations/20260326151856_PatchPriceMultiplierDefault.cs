using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConvenientStoreManagement.Migrations
{
    /// <inheritdoc />
    public partial class PatchPriceMultiplierDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Set PriceMultiplier = 1.5 for any existing products that still have 0
            migrationBuilder.Sql(
                "UPDATE [Products] SET [PriceMultiplier] = 1.5 WHERE [PriceMultiplier] = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally left blank – we can't recover the original 0 values
        }
    }
}
