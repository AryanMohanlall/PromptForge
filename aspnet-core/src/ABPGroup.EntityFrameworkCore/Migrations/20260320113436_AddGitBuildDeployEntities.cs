using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ABPGroup.Migrations
{
    /// <inheritdoc />
    public partial class AddGitBuildDeployEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Prompt",
                table: "CodeGenSessions",
                type: "character varying(10000)",
                maxLength: 10000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(5000)",
                oldMaxLength: 5000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedRequirement",
                table: "CodeGenSessions",
                type: "character varying(10000)",
                maxLength: 10000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(5000)",
                oldMaxLength: 5000,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "BuildJobs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    PromptVersion = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CurrentStep = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MetadataJson = table.Column<string>(type: "text", nullable: true),
                    DeploymentId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildJobs_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GeneratedArtifacts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    PromptVersion = table.Column<int>(type: "integer", nullable: false),
                    Path = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    FileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ArtifactType = table.Column<int>(type: "integer", nullable: false),
                    ContentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedArtifacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneratedArtifacts_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GitProfiles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Provider = table.Column<int>(type: "integer", nullable: false),
                    ProviderUserId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Username = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AvatarUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    AccessToken = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    IsConnected = table.Column<bool>(type: "boolean", nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GitProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GitProfiles_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectRepositories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    Provider = table.Column<int>(type: "integer", nullable: false),
                    Owner = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FullName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    DefaultBranch = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Visibility = table.Column<int>(type: "integer", nullable: false),
                    HtmlUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ExternalRepositoryId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectRepositories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectRepositories_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Deployments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    ProjectRepositoryId = table.Column<long>(type: "bigint", nullable: false),
                    Target = table.Column<int>(type: "integer", nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Url = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ProviderDeploymentId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TriggeredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deployments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deployments_ProjectRepositories_ProjectRepositoryId",
                        column: x => x.ProjectRepositoryId,
                        principalTable: "ProjectRepositories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Deployments_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RepositoryCommits",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectRepositoryId = table.Column<long>(type: "bigint", nullable: false),
                    Sha = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Branch = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PromptVersion = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepositoryCommits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepositoryCommits_ProjectRepositories_ProjectRepositoryId",
                        column: x => x.ProjectRepositoryId,
                        principalTable: "ProjectRepositories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeploymentLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeploymentId = table.Column<long>(type: "bigint", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    MetadataJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeploymentLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeploymentLogs_Deployments_DeploymentId",
                        column: x => x.DeploymentId,
                        principalTable: "Deployments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildJobs_ProjectId",
                table: "BuildJobs",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildJobs_ProjectId_PromptVersion",
                table: "BuildJobs",
                columns: new[] { "ProjectId", "PromptVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_DeploymentLogs_DeploymentId",
                table: "DeploymentLogs",
                column: "DeploymentId");

            migrationBuilder.CreateIndex(
                name: "IX_DeploymentLogs_Timestamp",
                table: "DeploymentLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Deployments_ProjectId",
                table: "Deployments",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Deployments_ProjectRepositoryId",
                table: "Deployments",
                column: "ProjectRepositoryId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedArtifacts_ProjectId",
                table: "GeneratedArtifacts",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedArtifacts_ProjectId_PromptVersion",
                table: "GeneratedArtifacts",
                columns: new[] { "ProjectId", "PromptVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_GitProfiles_UserId",
                table: "GitProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GitProfiles_UserId_Provider",
                table: "GitProfiles",
                columns: new[] { "UserId", "Provider" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRepositories_ProjectId",
                table: "ProjectRepositories",
                column: "ProjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RepositoryCommits_ProjectRepositoryId",
                table: "RepositoryCommits",
                column: "ProjectRepositoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RepositoryCommits_Sha",
                table: "RepositoryCommits",
                column: "Sha");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildJobs");

            migrationBuilder.DropTable(
                name: "DeploymentLogs");

            migrationBuilder.DropTable(
                name: "GeneratedArtifacts");

            migrationBuilder.DropTable(
                name: "GitProfiles");

            migrationBuilder.DropTable(
                name: "RepositoryCommits");

            migrationBuilder.DropTable(
                name: "Deployments");

            migrationBuilder.DropTable(
                name: "ProjectRepositories");

            migrationBuilder.AlterColumn<string>(
                name: "Prompt",
                table: "CodeGenSessions",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10000)",
                oldMaxLength: 10000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedRequirement",
                table: "CodeGenSessions",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10000)",
                oldMaxLength: 10000,
                oldNullable: true);
        }
    }
}
