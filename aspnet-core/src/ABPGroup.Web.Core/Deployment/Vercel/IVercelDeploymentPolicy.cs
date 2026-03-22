using Abp.Dependency;
using ABPGroup.Projects;

namespace ABPGroup.VercelDeployment
{
    public interface IVercelDeploymentPolicy
    {
        VercelDeploymentDecision Evaluate(bool autoDeploy, Framework framework, string repositoryFullName);
    }

    public class VercelDeploymentPolicy : IVercelDeploymentPolicy, ITransientDependency
    {
        public VercelDeploymentDecision Evaluate(bool autoDeploy, Framework framework, string repositoryFullName)
        {
            if (!autoDeploy)
            {
                return new VercelDeploymentDecision
                {
                    ShouldDeploy = false,
                    Reason = "Automatic deployment is disabled for this request."
                };
            }

            if (framework == Framework.DotNetBlazor)
            {
                return new VercelDeploymentDecision
                {
                    ShouldDeploy = false,
                    Reason = "Deployment skipped: .NET stacks are not deployed to Vercel."
                };
            }

            if (string.IsNullOrWhiteSpace(repositoryFullName))
            {
                return new VercelDeploymentDecision
                {
                    ShouldDeploy = false,
                    Reason = "Deployment skipped: repository information is incomplete."
                };
            }

            return new VercelDeploymentDecision
            {
                ShouldDeploy = true,
                Reason = null
            };
        }
    }
}