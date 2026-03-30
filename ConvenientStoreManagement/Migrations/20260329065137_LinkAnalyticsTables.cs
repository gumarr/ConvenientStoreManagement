using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConvenientStoreManagement.Migrations
{
    /// <inheritdoc />
    public partial class LinkAnalyticsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "DailySummaryStats",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "DailySummaryStats",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "AIRecommendations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailySummaryStats_CreatedBy",
                table: "DailySummaryStats",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AIRecommendations_UserId",
                table: "AIRecommendations",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AIRecommendations_Users_UserId",
                table: "AIRecommendations",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DailySummaryStats_Users_CreatedBy",
                table: "DailySummaryStats",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIRecommendations_Users_UserId",
                table: "AIRecommendations");

            migrationBuilder.DropForeignKey(
                name: "FK_DailySummaryStats_Users_CreatedBy",
                table: "DailySummaryStats");

            migrationBuilder.DropIndex(
                name: "IX_DailySummaryStats_CreatedBy",
                table: "DailySummaryStats");

            migrationBuilder.DropIndex(
                name: "IX_AIRecommendations_UserId",
                table: "AIRecommendations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "DailySummaryStats");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "DailySummaryStats");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AIRecommendations");
        }
    }
}
