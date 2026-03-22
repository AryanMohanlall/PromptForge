using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABPGroup.VercelDeployment
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

        /// <summary>
        /// Lists deployments from Vercel (GET /v6/deployments).
        /// All filters are optional.
        /// </summary>
        Task<VercelListResult<VercelDeploymentItem>> ListDeploymentsAsync(
            string projectId,
            string state,
            string branch,
            int limit = 20);

        /// <summary>
        /// Lists Vercel projects (GET /v10/projects).
        /// </summary>
        Task<VercelListResult<VercelProjectItem>> ListProjectsAsync(
            string search,
            int limit = 20);

        /// <summary>
        /// Redeploys an existing deployment by ID (POST /v13/deployments with deploymentId).
        /// </summary>
        Task<VercelDeploymentResult> RedeployAsync(
            string deploymentId,
            string projectName);
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

    public class VercelListResult<T>
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public List<T> Items { get; set; } = new List<T>();
    }

    public class VercelDeploymentItem
    {
        public string Uid { get; set; }
        public string Name { get; set; }
        public string ProjectId { get; set; }
        public string Url { get; set; }
        public string State { get; set; }
        public string Target { get; set; }
        public long Created { get; set; }
        public string InspectorUrl { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, string> Meta { get; set; }
        public VercelCreatorInfo Creator { get; set; }
    }

    public class VercelCreatorInfo
    {
        public string Uid { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }

    public class VercelProjectItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Framework { get; set; }
        public long UpdatedAt { get; set; }
    }
}
