using Abp.AspNetCore;
using Abp.AspNetCore.Configuration;
using Abp.AspNetCore.SignalR;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Zero.Configuration;
using ABPGroup.Authentication.External;
using ABPGroup.Authentication.JwtBearer;
using ABPGroup.Configuration;
using ABPGroup.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace ABPGroup
{
    [DependsOn(
         typeof(ABPGroupApplicationModule),
         typeof(ABPGroupEntityFrameworkModule),
         typeof(AbpAspNetCoreModule)
        , typeof(AbpAspNetCoreSignalRModule)
     )]
    public class ABPGroupWebCoreModule : AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public ABPGroupWebCoreModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void PreInitialize()
        {
            Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString(
                ABPGroupConsts.ConnectionStringName
            );

            // Use database for language management
            Configuration.Modules.Zero().LanguageManagement.EnableDbLocalization();

            Configuration.Modules.AbpAspNetCore()
                 .CreateControllersForAppServices(
                     typeof(ABPGroupApplicationModule).GetAssembly()
                 );

            ConfigureTokenAuth();
            ConfigureExternalAuth();
        }

        private void ConfigureTokenAuth()
        {
            IocManager.Register<TokenAuthConfiguration>();
            var tokenAuthConfig = IocManager.Resolve<TokenAuthConfiguration>();

            tokenAuthConfig.SecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appConfiguration["Authentication:JwtBearer:SecurityKey"]));
            tokenAuthConfig.Issuer = _appConfiguration["Authentication:JwtBearer:Issuer"];
            tokenAuthConfig.Audience = _appConfiguration["Authentication:JwtBearer:Audience"];
            tokenAuthConfig.SigningCredentials = new SigningCredentials(tokenAuthConfig.SecurityKey, SecurityAlgorithms.HmacSha256);
            tokenAuthConfig.Expiration = TimeSpan.FromDays(1);
        }

        private void ConfigureExternalAuth()
        {
            IocManager.Register<ExternalAuthConfiguration>();
            var externalAuthConfig = IocManager.Resolve<ExternalAuthConfiguration>();

            var githubClientId = _appConfiguration["GitHubOAuth:ClientId"] ?? _appConfiguration["GitHub:ClientId"];
            var githubClientSecret = _appConfiguration["GitHubOAuth:ClientSecret"] ?? _appConfiguration["GitHub:ClientSecret"];

            if (!string.IsNullOrWhiteSpace(githubClientId))
            {
                externalAuthConfig.Providers.Add(
                    new ExternalLoginProviderInfo(
                        "GitHub",
                        githubClientId,
                        githubClientSecret,
                        typeof(GitHubAuthProvider)));
            }
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(ABPGroupWebCoreModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            IocManager.Resolve<ApplicationPartManager>()
                .AddApplicationPartsIfNotAddedBefore(typeof(ABPGroupWebCoreModule).Assembly);
        }
    }
}
