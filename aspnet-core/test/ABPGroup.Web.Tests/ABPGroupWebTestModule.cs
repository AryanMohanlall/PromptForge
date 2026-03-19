using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using ABPGroup.EntityFrameworkCore;
using ABPGroup.Web.Host.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace ABPGroup.Web.Tests;

[DependsOn(
    typeof(ABPGroupWebHostModule),
    typeof(AbpAspNetCoreTestBaseModule)
)]
public class ABPGroupWebTestModule : AbpModule
{
    public ABPGroupWebTestModule(ABPGroupEntityFrameworkModule abpProjectNameEntityFrameworkModule)
    {
        abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
    }

    public override void PreInitialize()
    {
        Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(ABPGroupWebTestModule).GetAssembly());
    }

    public override void PostInitialize()
    {
        IocManager.Resolve<ApplicationPartManager>()
            .AddApplicationPartsIfNotAddedBefore(typeof(ABPGroupWebHostModule).Assembly);
    }
}