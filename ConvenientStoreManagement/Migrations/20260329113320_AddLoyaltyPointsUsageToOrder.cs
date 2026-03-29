using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConvenientStoreManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddLoyaltyPointsUsageToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LoyaltyPointsEarned",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LoyaltyPointsUsed",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MemberCardId",
                table: "Orders",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoyaltyPointsEarned",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "LoyaltyPointsUsed",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "MemberCardId",
                table: "Orders");
        }
    }
}
