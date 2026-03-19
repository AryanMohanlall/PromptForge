using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABPGroup.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeGenSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CodeGenSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    ProjectId = table.Column<long>(type: "bigint", nullable: true),
                    ProjectName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Prompt = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    NormalizedRequirement = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    DetectedFeaturesJson = table.Column<string>(type: "text", nullable: true),
                    DetectedEntitiesJson = table.Column<string>(type: "text", nullable: true),
                    ConfirmedStackJson = table.Column<string>(type: "text", nullable: true),
                    SpecJson = table.Column<string>(type: "text", nullable: true),
                    SpecConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GenerationStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GenerationCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ValidationResultsJson = table.Column<string>(type: "text", nullable: true),
                    ScaffoldTemplate = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    GeneratedFilesJson = table.Column<string>(type: "text", nullable: true),
                    RepairAttempts = table.Column<int>(type: "integer", nullable: false),
                    CurrentPhase = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CompletedStepsJson = table.Column<string>(type: "text", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeGenSessions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CodeGenSessions");
        }
    }
}
