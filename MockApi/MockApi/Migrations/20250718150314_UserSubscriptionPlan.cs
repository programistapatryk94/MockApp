using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MockApi.Migrations
{
    /// <inheritdoc />
    public partial class UserSubscriptionPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SubscriptionPlanPriceId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_SubscriptionPlanPriceId",
                table: "Users",
                column: "SubscriptionPlanPriceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_SubscriptionPlanPrices_SubscriptionPlanPriceId",
                table: "Users",
                column: "SubscriptionPlanPriceId",
                principalTable: "SubscriptionPlanPrices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_SubscriptionPlanPrices_SubscriptionPlanPriceId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_SubscriptionPlanPriceId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SubscriptionPlanPriceId",
                table: "Users");
        }
    }
}
