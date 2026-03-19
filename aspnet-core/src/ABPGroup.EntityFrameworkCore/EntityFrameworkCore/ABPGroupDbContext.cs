using Abp.Zero.EntityFrameworkCore;
using ABPGroup.Authorization.Roles;
using ABPGroup.Authorization.Users;
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
        });
    }
}
