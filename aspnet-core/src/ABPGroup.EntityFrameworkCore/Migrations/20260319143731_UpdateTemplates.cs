using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPGroup.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUpdatedAt",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "LikeCount",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "PreviewImageUrl",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "SourceUrl",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Templates");

            migrationBuilder.AlterColumn<string>(
                name: "Tags",
                table: "Templates",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Templates",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Templates",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.Sql(@"
    ALTER TABLE ""Templates"" ALTER COLUMN ""Category"" TYPE integer USING ""Category""::integer;
    ALTER TABLE ""Templates"" ALTER COLUMN ""Category"" SET DEFAULT 1;
    UPDATE ""Templates"" SET ""Category"" = 1 WHERE ""Category"" IS NULL;
    ALTER TABLE ""Templates"" ALTER COLUMN ""Category"" SET NOT NULL;
");

            migrationBuilder.AlterColumn<string>(
                name: "Author",
                table: "Templates",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Database",
                table: "Templates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ForkCount",
                table: "Templates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Framework",
                table: "Templates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IncludesAuth",
                table: "Templates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Templates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Language",
                table: "Templates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PreviewUrl",
                table: "Templates",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScaffoldConfig",
                table: "Templates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Templates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "Templates",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "Templates",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Templates_Category",
                table: "Templates",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_IsFeatured",
                table: "Templates",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_Status",
                table: "Templates",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Templates_Category",
                table: "Templates");

            migrationBuilder.DropIndex(
                name: "IX_Templates_IsFeatured",
                table: "Templates");

            migrationBuilder.DropIndex(
                name: "IX_Templates_Status",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "Database",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "ForkCount",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "Framework",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "IncludesAuth",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "PreviewUrl",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "ScaffoldConfig",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Templates");

            migrationBuilder.AlterColumn<string>(
                name: "Tags",
                table: "Templates",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Templates",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Templates",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Templates",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Author",
                table: "Templates",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedAt",
                table: "Templates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LikeCount",
                table: "Templates",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviewImageUrl",
                table: "Templates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Templates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceUrl",
                table: "Templates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Templates",
                type: "integer",
                nullable: true);
        }
    }
}
