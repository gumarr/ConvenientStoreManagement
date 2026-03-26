using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConvenientStoreManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddStatTablesAndCostSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ImportCostSnapshot",
                table: "OrderDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "DailyProductStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalQuantity = table.Column<int>(type: "int", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalImportCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalProfit = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyProductStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyProductStats_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DailySummaryStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalImportCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalProfit = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailySummaryStats", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyProductStats_ProductId_Date",
                table: "DailyProductStats",
                columns: new[] { "ProductId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailySummaryStats_Date",
                table: "DailySummaryStats",
                column: "Date",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyProductStats");

            migrationBuilder.DropTable(
                name: "DailySummaryStats");

            migrationBuilder.DropColumn(
                name: "ImportCostSnapshot",
                table: "OrderDetails");
        }
    }
}
