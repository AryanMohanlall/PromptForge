using System.Threading.Tasks;

namespace ABPGroup.Deployment.Vercel
{
    public interface IVercelDeploymentService
    {
        /// <summary>
        /// repoId — numeric GitHub repository ID from the GitHub API response
        /// (field "id" on repo create/get). Required by Vercel gitSource.
        /// </summary>
        Task<VercelDeploymentResult> TriggerDeploymentAsync(
            string repositoryFullName,
            long repoId,
            string branch,
            string projectName,
            string commitSha);
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