using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace ABPGroup.EntityFrameworkCore;

public static class ABPGroupDbContextConfigurer
{
    public static void Configure(DbContextOptionsBuilder<ABPGroupDbContext> builder, string connectionString)
    {
        builder.UseNpgsql(connectionString);
        builder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    public static void Configure(DbContextOptionsBuilder<ABPGroupDbContext> builder, DbConnection connection)
    {
        builder.UseNpgsql(connection);
        builder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }
}
