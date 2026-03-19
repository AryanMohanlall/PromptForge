using ABPGroup.Models.TokenAuth;
using ABPGroup.Web.Host.Controllers;
using Shouldly;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace ABPGroup.Web.Tests.Controllers;

public class HomeController_Tests : ABPGroupWebTestBase
{
    [Fact]
    public async Task Index_Test()
    {
        await AuthenticateAsync(null, new AuthenticateModel
        {
            UserNameOrEmailAddress = "admin",
            Password = "123qwe"
        });

        //Act
        var response = await GetResponseAsync(
            GetUrl<HomeController>(nameof(HomeController.Index)),
            HttpStatusCode.Found
        );

        //Assert
        response.Headers.Location?.ToString().ShouldBe("/swagger");
    }
}