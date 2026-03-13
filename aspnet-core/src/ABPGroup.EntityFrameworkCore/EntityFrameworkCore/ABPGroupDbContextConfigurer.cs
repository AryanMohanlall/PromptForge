using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace ABPGroup.EntityFrameworkCore;

public static class ABPGroupDbContextConfigurer
{
    public static void Configure(DbContextOptionsBuilder<ABPGroupDbContext> builder, string connectionString)
    {
        builder.UseNpgsql(connectionString);
    }

    public static void Configure(DbContextOptionsBuilder<ABPGroupDbContext> builder, DbConnection connection)
    {
        builder.UseNpgsql(connection);
    }
}
