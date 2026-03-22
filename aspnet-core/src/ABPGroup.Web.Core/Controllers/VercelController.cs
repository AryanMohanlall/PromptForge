using Abp.Authorization;
using ABPGroup.Deployment.Vercel;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ABPGroup.Controllers
{
    [ApiController]
    [AbpAuthorize]
    [Route("api/vercel")]
    public class VercelController : ABPGroupControllerBase
    {
        private readonly IVercelDeploymentService _vercelService;

        public class CreateDeploymentInput
        {
            /// <summary>GitHub owner/repo e.g. "acme/my-app"</summary>
            public string RepositoryFullName { get; set; }
            /// <summary>Numeric GitHub repository ID (required by Vercel gitSource)</summary>
            public long RepoId { get; set; }
            public string Branch { get; set; } = "main";
            public string ProjectName { get; set; }
            public string CommitSha { get; set; }
        }

        public class RedeployInput
        {
            /// <summary>Vercel deployment UID (e.g. "dpl_abc123")</summary>
            public string DeploymentId { get; set; }
            /// <summary>Project name used to label the new deployment</summary>
            public string ProjectName { get; set; }
        }

        public VercelController(IVercelDeploymentService vercelService)
        {
            _vercelService = vercelService;
        }

        /// <summary>
        /// Lists Vercel projects for the authenticated token.
        /// GET /api/vercel/projects?search=my-app&amp;limit=20
        /// </summary>
        [HttpGet("projects")]
        public async Task<IActionResult> GetProjects(
            [FromQuery] string search = null,
            [FromQuery] int limit = 20)
        {
            var result = await _vercelService.ListProjectsAsync(search, limit);
            if (!result.Success)
                return BadRequest(new { error = result.ErrorMessage });

            return Ok(new { projects = result.Items });
        }

        /// <summary>
        /// Lists deployments from Vercel, with optional filters.
        /// GET /api/vercel/deployments?projectId=prj_xxx&amp;state=READY&amp;branch=main&amp;limit=20
        /// </summary>
        [HttpGet("deployments")]
        public async Task<IActionResult> GetDeployments(
            [FromQuery] string projectId = null,
            [FromQuery] string state = null,
            [FromQuery] string branch = null,
            [FromQuery] int limit = 20)
        {
            var result = await _vercelService.ListDeploymentsAsync(projectId, state, branch, limit);
            if (!result.Success)
                return BadRequest(new { error = result.ErrorMessage });

            return Ok(new { deployments = result.Items });
        }

        /// <summary>
        /// Creates a new Vercel deployment from a GitHub repository.
        /// POST /api/vercel/deployments
        /// </summary>
        [HttpPost("deployments")]
        public async Task<IActionResult> CreateDeployment([FromBody] CreateDeploymentInput input)
        {
            if (string.IsNullOrWhiteSpace(input?.RepositoryFullName))
                return BadRequest(new { error = "RepositoryFullName is required." });

            if (input.RepoId <= 0)
                return BadRequest(new { error = "A valid numeric RepoId is required." });

            var result = await _vercelService.TriggerDeploymentAsync(
                input.RepositoryFullName,
                input.RepoId,
                input.Branch,
                input.ProjectName,
                input.CommitSha);

            if (!result.Triggered)
                return BadRequest(new { error = result.ErrorMessage });

            return Ok(new
            {
                deploymentId = result.DeploymentId,
                url = result.Url,
                inspectorUrl = result.InspectorUrl,
                state = result.State
            });
        }

        /// <summary>
        /// Redeploys an existing Vercel deployment by its UID.
        /// POST /api/vercel/deployments/redeploy
        /// </summary>
        [HttpPost("deployments/redeploy")]
        public async Task<IActionResult> Redeploy([FromBody] RedeployInput input)
        {
            if (string.IsNullOrWhiteSpace(input?.DeploymentId))
                return BadRequest(new { error = "DeploymentId is required." });

            var result = await _vercelService.RedeployAsync(input.DeploymentId, input.ProjectName);

            if (!result.Triggered)
                return BadRequest(new { error = result.ErrorMessage });

            return Ok(new
            {
                deploymentId = result.DeploymentId,
                url = result.Url,
                inspectorUrl = result.InspectorUrl,
                state = result.State
            });
        }
    }
}
