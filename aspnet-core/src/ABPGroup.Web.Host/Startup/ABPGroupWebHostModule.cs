using Abp.Modules;
using Abp.Reflection.Extensions;
using ABPGroup.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace ABPGroup.Web.Host.Startup
{
    [DependsOn(
       typeof(ABPGroupWebCoreModule))]
    public class ABPGroupWebHostModule : AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public ABPGroupWebHostModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void PreInitialize()
        {
            Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(ABPGroupWebHostModule).GetAssembly());
        }
    }
}
