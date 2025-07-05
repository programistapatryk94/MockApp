using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MockApi.Migrations
{
    /// <inheritdoc />
    public partial class AddApiPrefixToProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiPrefix",
                table: "Projects",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiPrefix",
                table: "Projects");
        }
    }
}
