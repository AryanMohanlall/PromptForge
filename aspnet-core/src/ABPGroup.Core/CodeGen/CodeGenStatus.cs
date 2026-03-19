namespace ABPGroup.CodeGen;

public enum CodeGenStatus
{
    Captured = 1,
    StackConfirmed = 2,
    SpecGenerated = 3,
    SpecConfirmed = 4,
    Generating = 5,
    Generated = 6,
    ValidationRunning = 7,
    ValidationPassed = 8,
    ValidationFailed = 9,
    Committed = 10,
    Deployed = 11
}
