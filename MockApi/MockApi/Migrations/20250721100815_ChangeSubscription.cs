using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MockApi.Migrations
{
    /// <inheritdoc />
    public partial class ChangeSubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSubscriptionCanceling",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "CurrentSubscriptions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionPlanPriceId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsCanceling = table.Column<bool>(type: "boolean", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifierUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrentSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurrentSubscriptions_SubscriptionPlanPrices_SubscriptionPla~",
                        column: x => x.SubscriptionPlanPriceId,
                        principalTable: "SubscriptionPlanPrices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CurrentSubscriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CurrentSubscriptions_SubscriptionPlanPriceId",
                table: "CurrentSubscriptions",
                column: "SubscriptionPlanPriceId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentSubscriptions_UserId",
                table: "CurrentSubscriptions",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurrentSubscriptions");

            migrationBuilder.AddColumn<bool>(
                name: "IsSubscriptionCanceling",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
