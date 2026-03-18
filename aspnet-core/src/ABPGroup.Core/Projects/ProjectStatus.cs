namespace ABPGroup.Projects;

public enum ProjectStatus
{
    Draft = 1,
    PromptSubmitted = 2,
    CodeGenerationInProgress = 3,
    RepositoryPushInProgress = 4,
    Deployed = 5,
    Failed = 6,
    Archived = 7
}
