using System.Collections.Generic;

namespace ABPGroup.CodeGen.Dto;

public class AppSpecDto
{
    public List<EntitySpecDto> Entities { get; set; } = new();
    public List<PageSpecDto> Pages { get; set; } = new();
    public List<ApiRouteSpecDto> ApiRoutes { get; set; } = new();
    public List<ValidationRuleDto> Validations { get; set; } = new();
    public List<FileEntryDto> FileManifest { get; set; } = new();

    // NEW: Enhanced spec fields
    public DependencyPlanDto DependencyPlan { get; set; } = new();
    public string ArchitectureNotes { get; set; } // high-level architecture description
}

public class EntitySpecDto
{
    public string Name { get; set; }
    public string TableName { get; set; }
    public List<FieldSpecDto> Fields { get; set; } = new();
    public List<RelationSpecDto> Relations { get; set; } = new();
}

public class FieldSpecDto
{
    public string Name { get; set; }
    public string Type { get; set; }
    public bool Required { get; set; }
    public bool? Unique { get; set; }
    public object Default { get; set; }
    public int? MaxLength { get; set; }
    public List<string> EnumValues { get; set; }
    public string Description { get; set; }
}

public class RelationSpecDto
{
    public string Type { get; set; }
    public string Target { get; set; }
    public string ForeignKey { get; set; }
}

public class PageSpecDto
{
    public string Route { get; set; }
    public string Name { get; set; }
    public string Layout { get; set; }
    public List<string> Components { get; set; } = new();
    public List<string> DataRequirements { get; set; } = new();
    public string Description { get; set; }
}

public class ApiRouteSpecDto
{
    public string Method { get; set; }
    public string Path { get; set; }
    public string Handler { get; set; }
    public object RequestBody { get; set; }
    public object ResponseShape { get; set; } = new();
    public bool Auth { get; set; }
    public string Description { get; set; }
}

public class ValidationRuleDto
{
    public string Id { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public string Target { get; set; }
    public string Assertion { get; set; }
    public bool Automatable { get; set; }
    public string Script { get; set; }
}

public class FileEntryDto
{
    public string Path { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
}
