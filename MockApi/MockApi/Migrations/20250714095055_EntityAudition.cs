using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MockApi.Migrations
{
    /// <inheritdoc />
    public partial class EntityAudition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mocks_Users_UserId",
                table: "Mocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Users_UserId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_UserId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Mocks_UserId",
                table: "Mocks");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Mocks");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorUserId",
                table: "Subscriptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierUserId",
                table: "Subscriptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "Subscriptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorUserId",
                table: "SubscriptionHistories",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierUserId",
                table: "SubscriptionHistories",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "SubscriptionHistories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorUserId",
                table: "Projects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierUserId",
                table: "Projects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "Projects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorUserId",
                table: "Mocks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierUserId",
                table: "Mocks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "Mocks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierUserId",
                table: "FeatureSettings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "FeatureSettings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CreatorUserId",
                table: "Projects",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_LastModifierUserId",
                table: "Projects",
                column: "LastModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Mocks_CreatorUserId",
                table: "Mocks",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Mocks_LastModifierUserId",
                table: "Mocks",
                column: "LastModifierUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mocks_Users_CreatorUserId",
                table: "Mocks",
                column: "CreatorUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Mocks_Users_LastModifierUserId",
                table: "Mocks",
                column: "LastModifierUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Users_CreatorUserId",
                table: "Projects",
                column: "CreatorUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Users_LastModifierUserId",
                table: "Projects",
                column: "LastModifierUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mocks_Users_CreatorUserId",
                table: "Mocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Mocks_Users_LastModifierUserId",
                table: "Mocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Users_CreatorUserId",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Users_LastModifierUserId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_CreatorUserId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_LastModifierUserId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Mocks_CreatorUserId",
                table: "Mocks");

            migrationBuilder.DropIndex(
                name: "IX_Mocks_LastModifierUserId",
                table: "Mocks");

            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "LastModifierUserId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                table: "SubscriptionHistories");

            migrationBuilder.DropColumn(
                name: "LastModifierUserId",
                table: "SubscriptionHistories");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "SubscriptionHistories");

            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "LastModifierUserId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                table: "Mocks");

            migrationBuilder.DropColumn(
                name: "LastModifierUserId",
                table: "Mocks");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "Mocks");

            migrationBuilder.DropColumn(
                name: "LastModifierUserId",
                table: "FeatureSettings");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "FeatureSettings");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Projects",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Mocks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Projects_UserId",
                table: "Projects",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Mocks_UserId",
                table: "Mocks",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mocks_Users_UserId",
                table: "Mocks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Users_UserId",
                table: "Projects",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
