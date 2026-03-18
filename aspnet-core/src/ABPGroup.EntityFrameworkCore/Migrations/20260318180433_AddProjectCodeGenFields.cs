using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPGroup.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectCodeGenFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ArchitectureSummary",
                table: "Projects",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeneratedModules",
                table: "Projects",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TemplateId",
                table: "Projects",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchitectureSummary",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "GeneratedModules",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "Projects");
        }
    }
}
