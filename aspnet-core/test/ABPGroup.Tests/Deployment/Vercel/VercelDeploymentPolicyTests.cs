using ABPGroup.VercelDeployment;
using ABPGroup.Projects;
using Xunit;

namespace ABPGroup.Tests.VercelDeployment
{
    public class VercelDeploymentPolicyTests
    {
        private readonly VercelDeploymentPolicy _policy = new VercelDeploymentPolicy();

        [Fact]
        public void Evaluate_AutoDeployDisabled_ShouldSkip()
        {
            var result = _policy.Evaluate(false, Framework.NextJS, "owner/repo");

            Assert.False(result.ShouldDeploy);
            Assert.Equal("Automatic deployment is disabled for this request.", result.Reason);
        }

        [Fact]
        public void Evaluate_DotNetFramework_ShouldSkip()
        {
            var result = _policy.Evaluate(true, Framework.DotNetBlazor, "owner/repo");

            Assert.False(result.ShouldDeploy);
            Assert.Equal("Deployment skipped: .NET stacks are not deployed to Vercel.", result.Reason);
        }

        [Fact]
        public void Evaluate_MissingRepositoryFullName_ShouldSkip()
        {
            var result = _policy.Evaluate(true, Framework.NextJS, "");

            Assert.False(result.ShouldDeploy);
            Assert.Equal("Deployment skipped: repository information is incomplete.", result.Reason);
        }

        [Fact]
        public void Evaluate_EligibleProject_ShouldDeploy()
        {
            var result = _policy.Evaluate(true, Framework.NextJS, "owner/repo");

            Assert.True(result.ShouldDeploy);
            Assert.Null(result.Reason);
        }
    }
}