using Abp.Dependency;
using ABPGroup.EntityFrameworkCore;
using ABPGroup.Identity;
using Castle.MicroKernel.Registration;
using Castle.Windsor.MsDependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ABPGroup.Tests.DependencyInjection;

public static class ServiceCollectionRegistrar
{
    public static void Register(IIocManager iocManager)
    {
        var services = new ServiceCollection();

        IdentityRegistrar.Register(services);

        services.AddEntityFrameworkInMemoryDatabase();
        services.AddHttpContextAccessor();

        var serviceProvider = WindsorRegistrationHelper.CreateServiceProvider(iocManager.IocContainer, services);

        var builder = new DbContextOptionsBuilder<ABPGroupDbContext>();
        builder.UseInMemoryDatabase(Guid.NewGuid().ToString()).UseInternalServiceProvider(serviceProvider);

        iocManager.IocContainer.Register(
            Component
                .For<DbContextOptions<ABPGroupDbContext>>()
                .Instance(builder.Options)
                .LifestyleSingleton()
        );
    }
}
