using System.Threading.Tasks;

namespace ABPGroup.Deployment.Vercel
{
    public interface IVercelDeploymentService
    {
        Task<VercelDeploymentResult> TriggerDeploymentAsync(string repositoryFullName, string branch, string projectName);
    }

    public class VercelDeploymentResult
    {
        public bool Triggered { get; set; }
        public string DeploymentId { get; set; }
        public string Url { get; set; }
        public string InspectorUrl { get; set; }
        public string State { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class VercelDeploymentDecision
    {
        public bool ShouldDeploy { get; set; }
        public string Reason { get; set; }
    }
}
