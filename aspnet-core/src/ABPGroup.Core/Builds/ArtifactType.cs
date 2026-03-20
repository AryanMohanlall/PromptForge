namespace ABPGroup.Builds
{
    /// <summary>
    /// The type of generated artifact produced during code generation.
    /// </summary>
    public enum ArtifactType
    {
        SourceCode = 1,
        Configuration = 2,
        Migration = 3,
        Test = 4,
        Documentation = 5,
        Asset = 6
    }
}
