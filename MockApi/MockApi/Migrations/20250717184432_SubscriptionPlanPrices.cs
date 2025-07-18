using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MockApi.Migrations
{
    /// <inheritdoc />
    public partial class SubscriptionPlanPrices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "StripePriceId",
                table: "SubscriptionPlans");

            migrationBuilder.CreateTable(
                name: "SubscriptionPlanPrices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    StripePriceId = table.Column<string>(type: "text", nullable: false),
                    SubscriptionPlanId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlanPrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionPlanPrices_SubscriptionPlans_SubscriptionPlanId",
                        column: x => x.SubscriptionPlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanPrices_SubscriptionPlanId",
                table: "SubscriptionPlanPrices",
                column: "SubscriptionPlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriptionPlanPrices");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "SubscriptionPlans",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "StripePriceId",
                table: "SubscriptionPlans",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
