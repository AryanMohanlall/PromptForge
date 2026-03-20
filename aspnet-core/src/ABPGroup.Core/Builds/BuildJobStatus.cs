namespace ABPGroup.Builds
{
    /// <summary>
    /// Current status of a build job.
    /// </summary>
    public enum BuildJobStatus
    {
        Queued = 1,
        Running = 2,
        Succeeded = 3,
        Failed = 4,
        Cancelled = 5
    }
}
