namespace ABPGroup.Builds
{
    /// <summary>
    /// The type of build job being executed.
    /// </summary>
    public enum BuildJobType
    {
        CodeGeneration = 1,
        Validation = 2,
        Deployment = 3,
        Repair = 4
    }
}
