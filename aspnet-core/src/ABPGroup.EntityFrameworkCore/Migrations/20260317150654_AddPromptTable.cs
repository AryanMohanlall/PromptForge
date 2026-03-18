using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ABPGroup.Migrations
{
    /// <inheritdoc />
    public partial class AddPromptTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PromptId",
                table: "Projects",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Prompts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prompts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prompts_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_PromptId",
                table: "Projects",
                column: "PromptId");

            migrationBuilder.CreateIndex(
                name: "IX_Prompts_ProjectId",
                table: "Prompts",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Prompts_ProjectId_Version",
                table: "Prompts",
                columns: new[] { "ProjectId", "Version" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Prompts_PromptId",
                table: "Projects",
                column: "PromptId",
                principalTable: "Prompts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Prompts_PromptId",
                table: "Projects");

            migrationBuilder.DropTable(
                name: "Prompts");

            migrationBuilder.DropIndex(
                name: "IX_Projects_PromptId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "PromptId",
                table: "Projects");
        }
    }
}
