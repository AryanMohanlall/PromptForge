using Abp.Zero.EntityFrameworkCore;
using ABPGroup.Authorization.Roles;
using ABPGroup.Authorization.Users;
using ABPGroup.MultiTenancy;
using ABPGroup.Projects;
using Microsoft.EntityFrameworkCore;

namespace ABPGroup.EntityFrameworkCore;

public class ABPGroupDbContext : AbpZeroDbContext<Tenant, Role, User, ABPGroupDbContext>
{
    /* Define a DbSet for each entity of the application */
    public DbSet<Project> Projects { get; set; }
    public DbSet<Prompt> Prompts { get; set; }

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
    }
}
