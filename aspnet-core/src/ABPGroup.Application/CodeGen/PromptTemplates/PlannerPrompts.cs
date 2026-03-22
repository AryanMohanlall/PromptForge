using System.Collections.Generic;
using ABPGroup.CodeGen.Dto;

namespace ABPGroup.CodeGen.PromptTemplates;

public static class PlannerPrompts
{
    private const string SpecContract = @"CRITICAL: Return ONLY valid JSON wrapped in delimiters:
===SPEC_JSON===
{...}
===END SPEC_JSON===

The JSON must have this exact structure:
{
  ""architectureNotes"": ""<high-level architecture description>"",
  ""entities"": [
    {
      ""name"": ""EntityName"",
      ""tableName"": ""entity_names"",
      ""fields"": [
        {
          ""name"": ""fieldName"",
          ""type"": ""string|int|float|boolean|datetime|enum|json"",
          ""required"": true|false,
          ""unique"": true|false,
          ""maxLength"": 255,
          ""enumValues"": [""value1"", ""value2""],
          ""description"": ""Field description""
        }
      ],
      ""relations"": [
        {
          ""type"": ""one-to-one|one-to-many|many-to-many"",
          ""target"": ""TargetEntity"",
          ""foreignKey"": ""targetId""
        }
      ]
    }
  ],
  ""pages"": [
    {
      ""route"": ""/path"",
      ""name"": ""PageName"",
      ""layout"": ""authenticated|public|admin"",
      ""components"": [""ComponentName""],
      ""dataRequirements"": [""entity.field""],
      ""description"": ""Page description""
    }
  ],
  ""apiRoutes"": [
    {
      ""method"": ""GET|POST|PUT|PATCH|DELETE"",
      ""path"": ""/api/resource"",
      ""handler"": ""handlerName"",
      ""requestBody"": {""field"": ""type""},
      ""responseShape"": {""field"": ""type""},
      ""auth"": true|false,
      ""description"": ""Route description""
    }
  ],
  ""validations"": [
    {
      ""id"": ""validation-id"",
      ""category"": ""file-exists|entity-schema|route-exists|build-passes|lint-passes|env-vars|test-passes|auth-guard|type-check|api-returns"",
      ""description"": ""Validation description"",
      ""target"": ""file/path"",
      ""assertion"": ""What must be true"",
      ""automatable"": true|false,
      ""script"": ""optional script""
    }
  ],
  ""fileManifest"": [
    {
      ""path"": ""relative/path/to/file.tsx"",
      ""type"": ""scaffold|generated|static"",
      ""description"": ""File purpose""
    }
  ],
  ""dependencyPlan"": {
    ""dependencies"": [
      {
        ""name"": ""package-name"",
        ""version"": ""^1.0.0"",
        ""purpose"": ""What this package is for"",
        ""isExisting"": false
      }
    ],
    ""devDependencies"": [
      {
        ""name"": ""package-name"",
        ""version"": ""^1.0.0"",
        ""purpose"": ""What this package is for"",
        ""isExisting"": false
      }
    ],
    ""envVars"": {
      ""DATABASE_URL"": ""PostgreSQL connection string"",
      ""NEXTAUTH_SECRET"": ""Secret for NextAuth.js session encryption""
    }
  }
}";

    private const string SpecRules = @"RULES:
1. All field.type values MUST be one of: string, int, float, boolean, datetime, enum, json
2. All apiRoute.method values MUST be one of: GET, POST, PUT, PATCH, DELETE (uppercase)
3. All page.layout values MUST be one of: authenticated, public, admin
4. All validation.category values MUST be one of: file-exists, entity-schema, route-exists, build-passes, lint-passes, env-vars, test-passes, auth-guard, type-check, api-returns
5. All fileEntry.type values MUST be one of: scaffold, generated, static
6. Generate complete, realistic entities with proper field types
7. Include all CRUD API routes for each entity
8. Include all necessary pages for the application
9. Include validation rules for build, auth, routes, and entity schemas
10. List all files that will be generated
11. For dependencyPlan, ONLY add packages NOT already in the scaffold baseline
12. Mark isExisting=true only for packages already in the scaffold
13. Use latest stable versions compatible with the scaffold
14. List ALL required environment variables with descriptions
15. For Next.js projects, assume the App Router (src/app). NEVER use or refer to the legacy /pages directory in validations, file manifests, or anywhere else.";

    /// <summary>
    /// Builds a structured implementation-plan prompt from the user's normalized requirement.
    /// </summary>
    public static string BuildSpecPrompt(
        string requirement,
        StackConfigDto stack,
        List<string> features,
        List<string> entities)
    {
        return BuildPrompt(
            "Generate a comprehensive application specification as a JSON object.",
            $"\n\nApplication: {requirement}\n"
            + $"Features: {string.Join(", ", features)}\n"
            + $"Entities: {string.Join(", ", entities)}\n"
            + BuildStackContext(stack));
    }

    /// <summary>
    /// Builds a structured implementation-plan prompt from the approved README that the user reviewed.
    /// </summary>
    public static string BuildPlanFromReadmePrompt(
        string readmeMarkdown,
        StackConfigDto stack,
        string requirement,
        List<string> features,
        List<string> entities)
    {
        return BuildPrompt(
            "Convert the approved README below into a concrete implementation plan. Treat the README as the source of truth for scope, architecture, routes, entities, dependencies, and environment variables. Use the supplemental requirement hints to recover any explicit domain nouns, user flows, or in-memory entities that the README may only describe in prose.",
            $"\n\nApproved README:\n{readmeMarkdown}\n\n"
            + $"Original requirement: {requirement}\n"
            + $"Detected features: {string.Join(", ", features ?? new List<string>())}\n"
            + $"Detected entities: {string.Join(", ", entities ?? new List<string>())}\n"
            + "If the app is client-only or does not use a database, still include the core domain entities used in memory (for example Task, TodoItem, Note) and the pages/routes needed to operate them.\n\n"
            + BuildStackContext(stack));
    }

    /// <summary>
    /// Builds a README generation prompt for the user to review before scaffolding.
    /// </summary>
    public static string BuildReadmePrompt(
        string projectName,
        string requirement,
        StackConfigDto stack,
        List<string> features,
        List<string> entities)
    {
        return $@"Generate a high-level README.md and implementation summary for the following project.
Project Name: {projectName}
User Requirements: {requirement}
Stack: {BuildStackContext(stack)}
Desired Features: {string.Join(", ", features)}
Core Entities: {string.Join(", ", entities)}

The response MUST include:
===SUMMARY===
A concise (2-3 sentence) summary of the project architecture and the user's primary goal.
===END SUMMARY===

===README===
A professional markdown README including:
- Project Title
- Tech Stack
- Folder Structure (based on the chosen framework)
- Core Features
- Database Schema Overview
- API endpoints (if applicable)
===END README===

Focus on clarity and strategic architecture. Use the provided stack (Framework: {stack?.Framework}) as the technical constraint.";
    }

    private static string BuildPrompt(string objective, string context)
    {
        return $@"You are an expert software architect. {objective}

{SpecContract}

{SpecRules}{context}";
    }

    private static string BuildStackContext(StackConfigDto stack)
    {
        return $"Stack: Framework={stack?.Framework}, Language={stack?.Language}, Styling={stack?.Styling}, Database={stack?.Database}, ORM={stack?.Orm}, Auth={stack?.Auth}";
    }
}
