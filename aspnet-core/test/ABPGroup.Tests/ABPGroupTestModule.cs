using Abp.AutoMapper;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Modules;
using Abp.Net.Mail;
using Abp.TestBase;
using Abp.Zero.Configuration;
using Abp.Zero.EntityFrameworkCore;
using ABPGroup.CodeGen;
using ABPGroup.EntityFrameworkCore;
using ABPGroup.Projects.Dto;
using ABPGroup.Tests.DependencyInjection;
using Castle.MicroKernel.Registration;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ABPGroup.Tests;

[DependsOn(
    typeof(ABPGroupApplicationModule),
    typeof(ABPGroupEntityFrameworkModule),
    typeof(AbpTestBaseModule)
    )]
public class ABPGroupTestModule : AbpModule
{
    public ABPGroupTestModule(ABPGroupEntityFrameworkModule abpProjectNameEntityFrameworkModule)
    {
        abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
        abpProjectNameEntityFrameworkModule.SkipDbSeed = true;
    }

    public override void PreInitialize()
    {
        Configuration.UnitOfWork.Timeout = TimeSpan.FromMinutes(30);
        Configuration.UnitOfWork.IsTransactional = false;

        // Disable static mapper usage since it breaks unit tests (see https://github.com/aspnetboilerplate/aspnetboilerplate/issues/2052)
        Configuration.Modules.AbpAutoMapper().UseStaticMapper = false;

        Configuration.BackgroundJobs.IsJobExecutionEnabled = false;

        // Use database for language management
        Configuration.Modules.Zero().LanguageManagement.EnableDbLocalization();

        RegisterFakeService<AbpZeroDbMigrator<ABPGroupDbContext>>();

        Configuration.ReplaceService<IEmailSender, NullEmailSender>(DependencyLifeStyle.Transient);
    }

    public override void Initialize()
    {
        ServiceCollectionRegistrar.Register(IocManager);
        
        // Register required framework services for tests  BEFORE service replacement
        RegisterMockHttpClientFactory();
        RegisterMockConfiguration();
        
        // Replace CodeGenAppService with mock AFTER assembly registration
        RegisterMockCodeGenService();
    }

    private void RegisterMockHttpClientFactory()
    {
        IocManager.IocContainer.Register(
            Component.For<IHttpClientFactory>()
                .UsingFactoryMethod(() => Substitute.For<IHttpClientFactory>())
                .LifestyleSingleton()
        );
    }

    private void RegisterMockConfiguration()
    {
        var config = Substitute.For<IConfiguration>();
        
        // Mock Groq API key
        config["Groq:ApiKey"].Returns("test-groq-key");
        config["Groq:Model"].Returns("llama-3.3-70b-versatile");
        
        // Mock CodeGen paths
        config["CodeGen:OutputPath"].Returns("/tmp/GeneratedApps");
        config["CodeGen:LocalCopyPath"].Returns("");
        config["CodeGen:SkipBuild"].Returns("true"); // Skip npm build in tests
        
        IocManager.IocContainer.Register(
            Component.For<IConfiguration>()
                .Instance(config)
                .LifestyleSingleton()
        );
    }

    private void RegisterMockCodeGenService()
    {
        // Create a mock CodeGenAppService that returns dummy generated files
        var mockService = Substitute.For<ICodeGenAppService>();
        
        mockService.GenerateProjectAsync(Arg.Any<CreateUpdateProjectDto>())
            .Returns(x =>
            {
                var request = (CreateUpdateProjectDto)x[0];
                return Task.FromResult(new CodeGenResult
                {
                    GeneratedProjectId = request.Id,
                    OutputPath = $"/tmp/GeneratedApps/{request.Name}",
                    Files = new List<GeneratedFile>
                    {
                        new GeneratedFile
                        {
                            Path = "package.json",
                            Content = @"{ ""name"": ""test-app"", ""version"": ""0.1.0"" }"
                        },
                        new GeneratedFile
                        {
                            Path = "src/app/page.tsx",
                            Content = "export default function Home() { return <h1>Test</h1>; }"
                        }
                    }
                });
            });
        
        // Register mock using proper Castle Windsor syntax
        IocManager.IocContainer.Register(
            Component.For<ICodeGenAppService>()
                .Instance(mockService)
                .IsDefault()
        );
    }

    private void RegisterFakeService<TService>() where TService : class
    {
        IocManager.IocContainer.Register(
            Component.For<TService>()
                .UsingFactoryMethod(() => Substitute.For<TService>())
                .LifestyleSingleton()
        );
    }
}
