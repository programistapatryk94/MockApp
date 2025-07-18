using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MockApi.Migrations
{
    /// <inheritdoc />
    public partial class SubscriptionPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    StripePriceId = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxProjects = table.Column<int>(type: "integer", nullable: false),
                    MaxResources = table.Column<int>(type: "integer", nullable: false),
                    HasCustomResponse = table.Column<bool>(type: "boolean", nullable: false),
                    HasCollaboration = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriptionPlans");
        }
    }
}
