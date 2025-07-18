using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MockApi.Migrations
{
    /// <inheritdoc />
    public partial class SubscriptionPlanPriceName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "SubscriptionPlanPrices",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "SubscriptionPlanPrices");
        }
    }
}
