namespace ABPGroup.Deployments
{
    /// <summary>
    /// Current status of a deployment.
    /// </summary>
    public enum DeploymentStatus
    {
        Pending = 1,
        InProgress = 2,
        Succeeded = 3,
        Failed = 4,
        RolledBack = 5
    }
}
