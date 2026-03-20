namespace ABPGroup.Deployments
{
    /// <summary>
    /// Target platform for a deployment.
    /// </summary>
    public enum DeploymentTarget
    {
        Vercel = 1,
        Netlify = 2,
        Railway = 3,
        Render = 4,
        AzureAppService = 5,
        Custom = 6
    }
}
