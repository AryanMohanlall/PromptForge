using Abp.Zero.EntityFrameworkCore;
using ABPGroup.Authorization.Roles;
using ABPGroup.Authorization.Users;
using ABPGroup.Builds;
using ABPGroup.CodeGen;
using ABPGroup.Deployments;
using ABPGroup.Git;
using ABPGroup.MultiTenancy;
using ABPGroup.Projects;
using ABPGroup.Templates;
using Microsoft.EntityFrameworkCore;

namespace ABPGroup.EntityFrameworkCore;

public class ABPGroupDbContext : AbpZeroDbContext<Tenant, Role, User, ABPGroupDbContext>
{
    /* Define a DbSet for each entity of the application */
    public DbSet<Project> Projects { get; set; }
    public DbSet<Prompt> Prompts { get; set; }
    public DbSet<Template> Templates { get; set; }
    public DbSet<UserFavoriteTemplate> UserFavoriteTemplates { get; set; }
    public DbSet<CodeGenSession> CodeGenSessions { get; set; }
    public DbSet<ProjectRepository> ProjectRepositories { get; set; }
    public DbSet<Deployment> Deployments { get; set; }

    public ABPGroupDbContext(DbContextOptions<ABPGroupDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Project>(b =>
        {
            b.ToTable("Projects");
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Prompt).IsRequired();
            b.HasIndex(x => x.WorkspaceId);
            b.HasIndex(x => x.PromptId);
            b.HasOne(x => x.Workspace)
                .WithMany()
                .HasForeignKey(x => x.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.PromptEntity)
                .WithMany()
                .HasForeignKey(x => x.PromptId)
                .OnDelete(DeleteBehavior.SetNull);
            b.Property(x => x.ArchitectureSummary).HasMaxLength(1000);
            b.Property(x => x.GeneratedModules).HasMaxLength(500);
            b.Property(x => x.StatusMessage).HasMaxLength(200);
        });

        modelBuilder.Entity<CodeGenSession>(b =>
        {
            b.ToTable("CodeGenSessions");
            b.HasKey(x => x.Id);
            b.Property(x => x.Prompt).HasMaxLength(10000);
            b.Property(x => x.NormalizedRequirement).HasMaxLength(10000);
            b.Property(x => x.ProjectName).HasMaxLength(128);
            b.Property(x => x.ScaffoldTemplate).HasMaxLength(256);
            b.Property(x => x.CurrentPhase).HasMaxLength(128);
            b.Property(x => x.ErrorMessage).HasMaxLength(1000);
        });

        modelBuilder.Entity<Prompt>(b =>
        {
            b.ToTable("Prompts");
            b.Property(x => x.Content).IsRequired();
            b.HasIndex(x => x.ProjectId);
            b.HasIndex(x => new { x.ProjectId, x.Version }).IsUnique();
            b.HasOne(x => x.Project)
                .WithMany(x => x.Prompts)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        #region Git

        modelBuilder.Entity<GitProfile>(b =>
        {
            b.ToTable("GitProfiles");
            b.Property(x => x.ProviderUserId).IsRequired().HasMaxLength(256);
            b.Property(x => x.Username).IsRequired().HasMaxLength(256);
            b.Property(x => x.AvatarUrl).HasMaxLength(1024);
            b.Property(x => x.AccessToken).HasMaxLength(2048);
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => new { x.UserId, x.Provider }).IsUnique();
            b.HasOne(x => x.User)
                .WithMany(x => x.GitProfiles)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProjectRepository>(b =>
        {
            b.ToTable("ProjectRepositories");
            b.Property(x => x.Owner).IsRequired().HasMaxLength(256);
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.FullName).IsRequired().HasMaxLength(512);
            b.Property(x => x.DefaultBranch).HasMaxLength(128);
            b.Property(x => x.HtmlUrl).HasMaxLength(1024);
            b.Property(x => x.ExternalRepositoryId).HasMaxLength(256);
            b.HasIndex(x => x.ProjectId).IsUnique();
            b.HasOne(x => x.Project)
                .WithOne(x => x.Repository)
                .HasForeignKey<ProjectRepository>(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RepositoryCommit>(b =>
        {
            b.ToTable("RepositoryCommits");
            b.Property(x => x.Sha).IsRequired().HasMaxLength(40);
            b.Property(x => x.Branch).HasMaxLength(256);
            b.Property(x => x.Message).HasMaxLength(1000);
            b.HasIndex(x => x.ProjectRepositoryId);
            b.HasIndex(x => x.Sha);
            b.HasOne(x => x.ProjectRepository)
                .WithMany(x => x.Commits)
                .HasForeignKey(x => x.ProjectRepositoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        #endregion

        #region Builds

        modelBuilder.Entity<BuildJob>(b =>
        {
            b.ToTable("BuildJobs");
            b.Property(x => x.CurrentStep).HasMaxLength(256);
            b.Property(x => x.ErrorMessage).HasMaxLength(2000);
            b.HasIndex(x => x.ProjectId);
            b.HasIndex(x => new { x.ProjectId, x.PromptVersion });
            b.HasOne(x => x.Project)
                .WithMany(x => x.BuildJobs)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GeneratedArtifact>(b =>
        {
            b.ToTable("GeneratedArtifacts");
            b.Property(x => x.Path).IsRequired().HasMaxLength(1024);
            b.Property(x => x.FileName).IsRequired().HasMaxLength(256);
            b.Property(x => x.ContentHash).HasMaxLength(64);
            b.HasIndex(x => x.ProjectId);
            b.HasIndex(x => new { x.ProjectId, x.PromptVersion });
            b.HasOne(x => x.Project)
                .WithMany(x => x.GeneratedArtifacts)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        #endregion

        #region Deployments

        modelBuilder.Entity<Deployment>(b =>
        {
            b.ToTable("Deployments");
            b.Property(x => x.EnvironmentName).HasMaxLength(128);
            b.Property(x => x.Url).HasMaxLength(1024);
            b.Property(x => x.ProviderDeploymentId).HasMaxLength(256);
            b.Property(x => x.ErrorMessage).HasMaxLength(2000);
            b.HasIndex(x => x.ProjectId);
            b.HasIndex(x => x.ProjectRepositoryId);
            b.HasOne(x => x.Project)
                .WithMany(x => x.Deployments)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.ProjectRepository)
                .WithMany(x => x.Deployments)
                .HasForeignKey(x => x.ProjectRepositoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DeploymentLog>(b =>
        {
            b.ToTable("DeploymentLogs");
            b.Property(x => x.Message).IsRequired().HasMaxLength(4000);
            b.Property(x => x.Source).HasMaxLength(256);
            b.HasIndex(x => x.DeploymentId);
            b.HasIndex(x => x.Timestamp);
            b.HasOne(x => x.Deployment)
                .WithMany(x => x.Logs)
                .HasForeignKey(x => x.DeploymentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        #endregion

        modelBuilder.Entity<Template>(builder =>
        {
            builder.ToTable("Templates");
            builder.Property(x => x.Name).IsRequired().HasMaxLength(128);
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.Author).HasMaxLength(128);
            builder.Property(x => x.Tags).HasMaxLength(500);
            builder.Property(x => x.ThumbnailUrl).HasMaxLength(500);
            builder.Property(x => x.PreviewUrl).HasMaxLength(500);
            builder.Property(x => x.Version).HasMaxLength(20);
            builder.Property(x => x.ScaffoldConfig).HasColumnType("nvarchar(max)");
            builder.Property(x => x.Category).HasConversion<int>().IsRequired();
            builder.Property(x => x.Framework).HasConversion<int>().IsRequired();
            builder.Property(x => x.Language).HasConversion<int>().IsRequired();
            builder.Property(x => x.Database).HasConversion<int>().IsRequired();
            builder.Property(x => x.Status).HasConversion<int>().IsRequired();
            builder.HasIndex(x => x.Category);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.IsFeatured);
        });

        modelBuilder.Entity<UserFavoriteTemplate>(builder =>
        {
            builder.ToTable("UserFavoriteTemplates");
            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.TemplateId);
            builder.HasIndex(x => new { x.UserId, x.TemplateId }).IsUnique();
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Template)
                .WithMany()
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
