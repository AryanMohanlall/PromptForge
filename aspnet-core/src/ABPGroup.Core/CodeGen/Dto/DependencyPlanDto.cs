using System.Collections.Generic;

namespace ABPGroup.CodeGen.Dto;

public class DependencyPlanDto
{
    public List<DependencyItemDto> Dependencies { get; set; } = new();
    public List<DependencyItemDto> DevDependencies { get; set; } = new();
    public Dictionary<string, string> EnvVars { get; set; } = new(); // name → description/default
}

public class DependencyItemDto
{
    public string Name { get; set; }
    public string Version { get; set; } // e.g. "^6.9.0"
    public string Purpose { get; set; }
    public bool IsExisting { get; set; } // true if already in scaffold
}
