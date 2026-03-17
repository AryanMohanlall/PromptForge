using Abp.Zero.EntityFrameworkCore;
using ABPGroup.Authorization.Roles;
using ABPGroup.Authorization.Users;
using ABPGroup.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace ABPGroup.EntityFrameworkCore;

public class ABPGroupDbContext : AbpZeroDbContext<Tenant, Role, User, ABPGroupDbContext>
{
    /* Define a DbSet for each entity of the application */

    public ABPGroupDbContext(DbContextOptions<ABPGroupDbContext> options)
        : base(options)
    {
    }
}
