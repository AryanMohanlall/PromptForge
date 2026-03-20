using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ABPGroup.Migrations
{
    /// <inheritdoc />
    public partial class UserFavoriteTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Templates",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserFavoriteTemplates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    TemplateId = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavoriteTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFavoriteTemplates_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFavoriteTemplates_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteTemplates_TemplateId",
                table: "UserFavoriteTemplates",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteTemplates_UserId",
                table: "UserFavoriteTemplates",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteTemplates_UserId_TemplateId",
                table: "UserFavoriteTemplates",
                columns: new[] { "UserId", "TemplateId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFavoriteTemplates");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Templates");
        }
    }
}
