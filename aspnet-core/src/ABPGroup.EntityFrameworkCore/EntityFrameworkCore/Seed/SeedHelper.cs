using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Uow;
using Abp.MultiTenancy;
using ABPGroup.EntityFrameworkCore.Seed.Host;
using ABPGroup.EntityFrameworkCore.Seed.Tenants;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Transactions;

namespace ABPGroup.EntityFrameworkCore.Seed;

public static class SeedHelper
{
    public static void SeedHostDb(IIocResolver iocResolver)
    {
        WithDbContext<ABPGroupDbContext>(iocResolver, SeedHostDb);
    }

    public static void SeedHostDb(ABPGroupDbContext context)
    {
        context.SuppressAutoSetTenantId = true;

        // Legacy SQL Server migrations can leave a partially-initialized PostgreSQL DB.
        // If core ABP tables are missing, rebuild schema from the current EF model.
        // If ABP tables exist but newer domain tables are missing, apply pending migrations.
        if (context.Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true)
        {
            var hasAbpEditions = PostgreSqlTableExists(context, "AbpEditions");
            var hasProjects = PostgreSqlTableExists(context, "Projects");

            if (!hasAbpEditions)
            {
                context.Database.ExecuteSqlRaw("DROP SCHEMA IF EXISTS public CASCADE;");
                context.Database.ExecuteSqlRaw("CREATE SCHEMA public;");
                context.Database.EnsureCreated();
            }
            else if (!hasProjects)
            {
                context.Database.Migrate();
            }
            else
            {
                context.Database.EnsureCreated();
            }
        }
        else
        {
            context.Database.EnsureCreated();
        }

        // Host seed
        new InitialHostDbBuilder(context).Create();

        // Default tenant seed (in host database).
        new DefaultTenantBuilder(context).Create();
        new TenantRoleAndUserBuilder(context, 1).Create();
    }

    private static void WithDbContext<TDbContext>(IIocResolver iocResolver, Action<TDbContext> contextAction)
        where TDbContext : DbContext
    {
        using (var uowManager = iocResolver.ResolveAsDisposable<IUnitOfWorkManager>())
        {
            using (var uow = uowManager.Object.Begin(TransactionScopeOption.Suppress))
            {
                var context = uowManager.Object.Current.GetDbContext<TDbContext>(MultiTenancySides.Host);

                contextAction(context);

                uow.Complete();
            }
        }
    }

    private static bool PostgreSqlTableExists(DbContext context, string tableName)
    {
        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
        {
            connection.Open();
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT to_regclass(@tableName)::text IS NOT NULL;";

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@tableName";
            parameter.Value = '"' + tableName + '"';
            command.Parameters.Add(parameter);

            var result = command.ExecuteScalar();
            return result is bool exists && exists;
        }
        finally
        {
            if (shouldClose)
            {
                connection.Close();
            }
        }
    }
}
