using System.Collections.Generic;
using System.Linq;
using ABPGroup.CodeGen.Dto;

namespace ABPGroup.CodeGen.PromptTemplates;

public static class GeneratorPrompts
{
    /// <summary>
    /// Builds the full-generation prompt for flows that generate the entire application in one shot.
    /// </summary>
    public static string BuildFullGenerationPrompt(
        AppSpecDto spec,
        StackConfigDto stack,
        string scaffoldBaseline,
        string approvedReadme = null)
    {
        return BuildLayerPrompt("full application", spec, stack, scaffoldBaseline, approvedReadme);
    }

    /// <summary>
    /// Builds a layer-specific generation prompt using the reviewed plan and scaffold as the source of truth.
    /// </summary>
    public static string BuildLayerPrompt(
        string layerDescription,
        AppSpecDto spec,
        StackConfigDto stack,
        string scaffoldBaseline,
        string approvedReadme = null)
    {
        spec ??= new AppSpecDto();
        scaffoldBaseline ??= string.Empty;

        var framework = stack?.Framework ?? "Next.js";
        var approvedReadmeSection = string.IsNullOrWhiteSpace(approvedReadme)
            ? string.Empty
            : $@"APPROVED README (use this exact reviewed scope during scaffolding and generation):
{approvedReadme}

";

        return $@"You are a principal full-stack engineer generating COMPLETE, RUNNABLE, PRODUCTION-SAFE {layerDescription} for a {framework} application.

Your output will be parsed automatically. You MUST follow the exact response format.

NON-NEGOTIABLE RULES
1. Generate real, working code only. No TODOs, placeholders, pseudo-code, mock implementations, or omitted sections.
2. Every import must resolve. Every referenced file, route, schema, API, env var, component, and utility must either already exist in the provided context or be created in your output.
3. The result must compile and run inside the provided scaffold/template. Do not invent a different architecture than the scaffold supports.
4. Use the latest stable dependency versions COMPATIBLE with the chosen framework and scaffold.
5. If the scaffold already includes a package.json, tsconfig, prisma schema, layout, theme, or config files, treat those versions and conventions as the baseline source of truth. Only add or update packages when required for the requested feature set.
6. Prefer current, non-deprecated APIs for the selected stack. Avoid legacy patterns.
7. If auth is required, wire the full flow end-to-end: session/token handling, protected routes, login state, and server/client boundaries.
8. If database access is required, ensure schema, data access, and API usage are consistent with each other.
9. If an API route is created, the frontend must call the correct path and shape. If the frontend calls a route, that route must exist.
10. If environment variables are required, include the relevant .env.example or config placeholders in generated files.
11. Do not break the scaffold's build system, linting assumptions, routing conventions, or framework version compatibility.
12. Optimize for a WORKING APPLICATION over cleverness.
13. If generating for Next.js, you MUST use the App Router (src/app directory structure). DO NOT output files to a legacy /pages directory.

SCAFFOLD BASELINE (treat as source of truth for existing files):
{scaffoldBaseline}

{approvedReadmeSection}SPECIFICATION:
Entities: {string.Join(", ", spec.Entities?.Select(e => e.Name) ?? new List<string>())}
Pages: {string.Join(", ", spec.Pages?.Select(p => p.Route) ?? new List<string>())}
API Routes: {string.Join(", ", spec.ApiRoutes?.Select(r => $"{r.Method} {r.Path}") ?? new List<string>())}

{(spec.DependencyPlan?.Dependencies?.Count > 0 ? $@"ADDITIONAL DEPENDENCIES TO ADD:
{string.Join("\n", spec.DependencyPlan.Dependencies.Where(d => !d.IsExisting).Select(d => $"- {d.Name}@{d.Version} ({d.Purpose})"))}" : string.Empty)}

{(spec.DependencyPlan?.EnvVars?.Count > 0 ? $@"REQUIRED ENVIRONMENT VARIABLES:
{string.Join("\n", spec.DependencyPlan.EnvVars.Select(e => $"- {e.Key}: {e.Value}"))}" : string.Empty)}

QUALITY BAR
- Complete file contents, not partial snippets
- Type-safe code
- Correct imports and exports
- Loading, empty, success, and error states where relevant
- Minimal but real validation
- Accessible and responsive UI
- Sensible defaults
- No dead code
- No duplicated logic when a utility/component can be shared

DEPENDENCY POLICY
- Prefer the scaffold's existing dependencies first.
- Add the fewest extra packages necessary.
- When adding packages, choose current stable versions compatible with the scaffold.
- If a dependency change is required, also output the necessary package/config file changes.

RETURN FORMAT
===ARCHITECTURE===
<brief description of this layer and how it fits the app>
===END ARCHITECTURE===

===MODULES===
<comma-separated module names>
===END MODULES===

Then for EACH file:

===FILE===
<file path relative to project root>
===CONTENT===
<full file content>
===END FILE===

FINAL SELF-CHECK BEFORE ANSWERING
- Would this compile?
- Would imports resolve?
- Would routes and APIs line up?
- Would the app run without placeholder code?
- Did I keep dependency choices compatible with the scaffold?
If any answer is no, fix it before returning.";
    }
    public static string BuildCodeGenSystemPrompt(
        string layerDescription,
        AppSpecDto spec,
        string framework,
        string scaffoldBaseline,
        string approvedReadme)
    {
        return $@"You are a principal full-stack engineer generating {layerDescription} for a {framework} application.
SCAFFOLD BASELINE:
{scaffoldBaseline}

APPROVED README:
{approvedReadme}

SPECIFICATION:
Entities: {string.Join(", ", spec?.Entities?.Select(e => e.Name) ?? new List<string>())}
Pages: {string.Join(", ", spec?.Pages?.Select(p => p.Route) ?? new List<string>())}

Follow the architecture defined in the scaffold and README.";
    }

    public static string BuildLayerUserPrompt(
        string userInstruction,
        System.Text.StringBuilder context,
        string originalPrompt,
        string approvedReadme)
    {
        return $@"Instruction: {userInstruction}
Original Requirement: {originalPrompt}
Context: {context}

Generate the code for this layer. Return the files in the standard delimiters.";
    }
}
