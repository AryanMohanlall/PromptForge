using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPGroup.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "AbpUsers",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "AbpUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GitHubAccessToken",
                table: "AbpUsers",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GitHubUsername",
                table: "AbpUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "AbpUsers",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "AvatarUrl", table: "AbpUsers");
            migrationBuilder.DropColumn(name: "DisplayName", table: "AbpUsers");
            migrationBuilder.DropColumn(name: "GitHubAccessToken", table: "AbpUsers");
            migrationBuilder.DropColumn(name: "GitHubUsername", table: "AbpUsers");
            migrationBuilder.DropColumn(name: "Role", table: "AbpUsers");
        }
    }
}
