using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABPGroup.CodeGen.Dto;

namespace ABPGroup.CodeGen.PromptTemplates;

public static class GeneratorPrompts
{
    /// <summary>
    /// Default required files for Next.js App Router projects.
    /// </summary>
    public static readonly List<string> DefaultNextJsRequiredFiles = new()
    {
        "package.json",
        "tsconfig.json",
        "next.config.mjs",
        "src/app/layout.tsx",
        "src/app/page.tsx",
        "prisma/schema.prisma",
        ".env.example"
    };

    /// <summary>
    /// Default required files for React Vite projects.
    /// </summary>
    public static readonly List<string> DefaultViteRequiredFiles = new()
    {
        "package.json",
        "tsconfig.json",
        "index.html",
        "src/App.tsx",
        "src/main.tsx",
        ".env.example"
    };

    /// <summary>
    /// Self-check rules that the AI must evaluate.
    /// </summary>
    public static readonly List<(string Rule, string Description)> SelfCheckRules = new()
    {
        ("all-imports-resolve", "Every import in every generated file points to a file that exists in the output or the scaffold baseline."),
        ("no-todos-or-placeholders", "No TODO, FIXME, placeholder, or mock code exists in any generated file."),
        ("scaffold-compatibility", "Output does not break the scaffold build system, routing conventions, or framework version."),
        ("routes-and-apis-aligned", "Every API route in the spec has a corresponding handler file. Every frontend call targets an existing route."),
        ("dependencies-compatible", "All added packages in package.json are compatible with the scaffold. No version conflicts."),
        ("schema-consistent", "Prisma schema entities match the spec. Field types are valid. Relations are correctly defined."),
        ("auth-wired-end-to-end", "If auth is required, session/token handling, protected routes, and login state are fully wired."),
        ("env-vars-declared", "All required environment variables are listed in .env.example with descriptions.")
    };

    /// <summary>
    /// Builds the full-generation prompt for flows that generate the entire application in one shot.
    /// </summary>
    public static string BuildFullGenerationPrompt(
        AppSpecDto spec,
        StackConfigDto stack,
        string scaffoldBaseline,
        string approvedReadme = null,
        List<string> requiredFiles = null,
        string fewShotExample = null,
        string knownFailures = null,
        StackCompatibilityResultDto compatibilityResult = null)
    {
        return BuildLayerPrompt("full application", spec, stack, scaffoldBaseline, approvedReadme, requiredFiles, fewShotExample, knownFailures, compatibilityResult);
    }

    /// <summary>
    /// Builds a layer-specific generation prompt using the reviewed plan and scaffold as the source of truth.
    /// Returns structured JSON output with embedded self-check validation.
    /// </summary>
    public static string BuildLayerPrompt(
        string layerDescription,
        AppSpecDto spec,
        StackConfigDto stack,
        string scaffoldBaseline,
        string approvedReadme = null,
        List<string> requiredFiles = null,
        string fewShotExample = null,
        string knownFailures = null,
        StackCompatibilityResultDto compatibilityResult = null)
    {
        spec ??= new AppSpecDto();
        scaffoldBaseline ??= string.Empty;

        var framework = stack?.Framework ?? "Next.js";
        var resolvedRequiredFiles = ResolveRequiredFiles(requiredFiles, framework);

        var sb = new StringBuilder();

        // ── ROLE ──
        sb.AppendLine($@"You are a principal full-stack engineer generating COMPLETE, RUNNABLE, PRODUCTION-SAFE {layerDescription} for a {framework} application.

Your output will be parsed as JSON. You MUST return a valid JSON object matching the schema defined below. Do NOT include any text outside the JSON object.");

        // ── STACK CONTEXT ──
        if (stack != null)
        {
            sb.AppendLine($@"
STACK CONFIGURATION:
- Framework: {stack.Framework}
- Language: {stack.Language}
- Styling: {stack.Styling}
- Database: {stack.Database}
- ORM: {stack.Orm}
- Auth: {stack.Auth}");
        }

        // ── STACK COMPATIBILITY VIOLATIONS ──
        if (compatibilityResult != null && !compatibilityResult.IsValid)
        {
            sb.AppendLine($@"
{compatibilityResult.FormatForPrompt()}");
        }

        // ── APPROVED README ──
        if (!string.IsNullOrWhiteSpace(approvedReadme))
        {
            sb.AppendLine($@"
APPROVED README (use this exact reviewed scope during scaffolding and generation):
{approvedReadme}");
        }

        // ── SCAFFOLD BASELINE ──
        sb.AppendLine($@"
SCAFFOLD BASELINE (treat as source of truth for existing files):
{scaffoldBaseline}");

        // ── SPECIFICATION ──
        sb.AppendLine($@"
SPECIFICATION:
Entities: {string.Join(", ", spec.Entities?.Select(e => e.Name) ?? new List<string>())}
Pages: {string.Join(", ", spec.Pages?.Select(p => p.Route) ?? new List<string>())}
API Routes: {string.Join(", ", spec.ApiRoutes?.Select(r => $"{r.Method} {r.Path}") ?? new List<string>())}");

        if (spec.DependencyPlan?.Dependencies?.Count > 0)
        {
            sb.AppendLine($@"
ADDITIONAL DEPENDENCIES TO ADD:
{string.Join("\n", spec.DependencyPlan.Dependencies.Where(d => !d.IsExisting).Select(d => $"- {d.Name}@{d.Version} ({d.Purpose})"))}");
        }

        if (spec.DependencyPlan?.EnvVars?.Count > 0)
        {
            sb.AppendLine($@"
REQUIRED ENVIRONMENT VARIABLES:
{string.Join("\n", spec.DependencyPlan.EnvVars.Select(e => $"- {e.Key}: {e.Value}"))}");
        }

        // ── REQUIRED FILES CHECKLIST ──
        sb.AppendLine($@"
REQUIRED FILES CHECKLIST:
You MUST generate ALL of the following files. Each file listed here must appear in your ""files"" array.");
        foreach (var file in resolvedRequiredFiles)
        {
            sb.AppendLine($"- [ ] {file}");
        }

        // ── KNOWN FAILURES ──
        if (!string.IsNullOrWhiteSpace(knownFailures))
        {
            sb.AppendLine($@"
KNOWN FAILURES FROM PREVIOUS ATTEMPTS (avoid these patterns):
{knownFailures}");
        }

        // ── FEW-SHOT EXAMPLE ──
        if (!string.IsNullOrWhiteSpace(fewShotExample))
        {
            sb.AppendLine($@"
FEW-SHOT EXAMPLE (a successful generation for a similar project — follow this structure):
{fewShotExample}");
        }

        // ── RULES ──
        sb.AppendLine(@"
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
14. You MUST include every file from the REQUIRED FILES CHECKLIST in your output.

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
- If a dependency change is required, also output the necessary package/config file changes.");

        // ── OUTPUT FORMAT ──
        sb.AppendLine(@"
OUTPUT FORMAT
You MUST return a single valid JSON object. No markdown fences, no commentary before or after.

JSON SCHEMA:
{
  ""architecture"": ""<brief description of this layer and how it fits the app>"",
  ""modules"": [""<comma-separated module names>""],
  ""files"": [
    {
      ""path"": ""<file path relative to project root>"",
      ""content"": ""<full file content>""
    }
  ],
  ""requiredFiles"": [""<list of all file paths you committed to generating>""
  ],
  ""selfCheck"": {
    ""passed"": true|false,
    ""checks"": [
      {
        ""rule"": ""all-imports-resolve"",
        ""passed"": true|false,
        ""notes"": ""<brief explanation>""
      },
      {
        ""rule"": ""no-todos-or-placeholders"",
        ""passed"": true|false,
        ""notes"": ""<brief explanation>""
      },
      {
        ""rule"": ""scaffold-compatibility"",
        ""passed"": true|false,
        ""notes"": ""<brief explanation>""
      },
      {
        ""rule"": ""routes-and-apis-aligned"",
        ""passed"": true|false,
        ""notes"": ""<brief explanation>""
      },
      {
        ""rule"": ""dependencies-compatible"",
        ""passed"": true|false,
        ""notes"": ""<brief explanation>""
      },
      {
        ""rule"": ""schema-consistent"",
        ""passed"": true|false,
        ""notes"": ""<brief explanation>""
      },
      {
        ""rule"": ""auth-wired-end-to-end"",
        ""passed"": true|false,
        ""notes"": ""<brief explanation>""
      },
      {
        ""rule"": ""env-vars-declared"",
        ""passed"": true|false,
        ""notes"": ""<brief explanation>""
      }
    ]
  }
}

SELF-CHECK RULES (evaluate each honestly before returning):
1. all-imports-resolve: Every import in every generated file points to a file that exists in the output or the scaffold baseline.
2. no-todos-or-placeholders: No TODO, FIXME, placeholder, or mock code exists in any generated file.
3. scaffold-compatibility: Output does not break the scaffold build system, routing conventions, or framework version.
4. routes-and-apis-aligned: Every API route in the spec has a corresponding handler file. Every frontend call targets an existing route.
5. dependencies-compatible: All added packages in package.json are compatible with the scaffold. No version conflicts.
6. schema-consistent: Prisma schema entities match the spec. Field types are valid. Relations are correctly defined.
7. auth-wired-end-to-end: If auth is required, session/token handling, protected routes, and login state are fully wired.
8. env-vars-declared: All required environment variables are listed in .env.example with descriptions.

IMPORTANT: If any self-check rule fails, set ""passed"": false on the overall selfCheck and on the failing rule. Fix the issue in your code before returning. The output will be validated server-side.");

        return sb.ToString();
    }

    /// <summary>
    /// Builds a code generation system prompt for the layer-based generation flow.
    /// All structural information (folder layout, entities, pages, routes, validations) is derived
    /// dynamically from the approved README and spec — nothing is hardcoded.
    /// </summary>
    public static string BuildCodeGenSystemPrompt(
        string layerDescription,
        AppSpecDto spec,
        StackConfigDto stack,
        string scaffoldBaseline,
        string approvedReadme,
        string existingLayerMetadata = null,
        List<string> requiredFiles = null,
        string fewShotExample = null,
        string knownFailures = null)
    {
        spec ??= new AppSpecDto();
        var framework = stack?.Framework ?? "Next.js";
        var sb = new StringBuilder();

        // ── ROLE ──
        sb.AppendLine($@"You are a principal full-stack engineer generating the {layerDescription} for a production application.

Your output will be parsed as JSON. You MUST return a single valid JSON object matching the schema defined below. Do NOT include any text outside the JSON object.");

        // ── STACK CONFIGURATION ──
        if (stack != null)
        {
            sb.AppendLine($@"
STACK CONFIGURATION:
- Framework: {stack.Framework}
- Language: {stack.Language}
- Styling: {stack.Styling}
- Database: {stack.Database}
- ORM: {stack.Orm}
- Auth: {stack.Auth}");
        }

        // ── APPROVED README (folder structure is derived from here, NOT hardcoded) ──
        if (!string.IsNullOrWhiteSpace(approvedReadme))
        {
            sb.AppendLine($@"
APPROVED README (This is your ARCHITECTURAL BLUEPRINT and the SINGLE SOURCE OF TRUTH for folder structure, conventions, and project scope. You MUST follow the folder structure defined here exactly. Do NOT invent directories or file locations that are not described in this README):
{approvedReadme}");
        }

        // ── SCAFFOLD BASELINE ──
        if (!string.IsNullOrWhiteSpace(scaffoldBaseline))
        {
            sb.AppendLine($@"
SCAFFOLD BASELINE (existing files in the project — treat as immutable source of truth. Match their conventions, versions, and patterns):
{scaffoldBaseline}");
        }

        // ── PREVIOUSLY GENERATED LAYERS ──
        if (!string.IsNullOrWhiteSpace(existingLayerMetadata))
        {
            sb.AppendLine($@"
PREVIOUSLY GENERATED LAYERS (these files already exist and are FINAL — you MUST:
  1. Import from them using their exact export names
  2. Match their exact types, interfaces, and field names
  3. Reference their actual route paths and handler signatures
  4. Do NOT re-generate, rename, or contradict any of these files
  5. If you create a frontend page that calls an API, use the exact route path and request/response shape from the backend layer below
  6. If you create an API handler that queries the database, use the exact model names and field names from the database layer below):
{existingLayerMetadata}");
        }

        // ── FULL SPECIFICATION (all rich data from the plan, not flattened) ──
        sb.AppendLine("\nSPECIFICATION:");

        // Architecture notes
        if (!string.IsNullOrWhiteSpace(spec.ArchitectureNotes))
        {
            sb.AppendLine($"Architecture: {spec.ArchitectureNotes}");
        }

        // Entities — full field-level detail so the AI knows exact types, constraints, relations
        if (spec.Entities?.Count > 0)
        {
            sb.AppendLine("\nENTITIES (generate schema models that match these EXACTLY):");
            foreach (var entity in spec.Entities)
            {
                sb.AppendLine($"\n  Entity: {entity.Name}" + (!string.IsNullOrWhiteSpace(entity.TableName) ? $" (table: {entity.TableName})" : ""));
                if (entity.Fields?.Count > 0)
                {
                    sb.AppendLine("  Fields:");
                    foreach (var field in entity.Fields)
                    {
                        var constraints = new List<string>();
                        if (field.Required) constraints.Add("required");
                        if (field.Unique == true) constraints.Add("unique");
                        if (field.MaxLength.HasValue) constraints.Add($"maxLength={field.MaxLength}");
                        if (field.Default != null) constraints.Add($"default={field.Default}");
                        if (field.EnumValues?.Count > 0) constraints.Add($"enum=[{string.Join(", ", field.EnumValues)}]");
                        var constraintStr = constraints.Count > 0 ? $" ({string.Join(", ", constraints)})" : "";
                        var descStr = !string.IsNullOrWhiteSpace(field.Description) ? $" // {field.Description}" : "";
                        sb.AppendLine($"    - {field.Name}: {field.Type}{constraintStr}{descStr}");
                    }
                }
                if (entity.Relations?.Count > 0)
                {
                    sb.AppendLine("  Relations:");
                    foreach (var rel in entity.Relations)
                    {
                        sb.AppendLine($"    - {rel.Type} -> {rel.Target}" + (!string.IsNullOrWhiteSpace(rel.ForeignKey) ? $" (FK: {rel.ForeignKey})" : ""));
                    }
                }
            }
        }

        // Pages — full detail so the AI knows layouts, components, data requirements
        if (spec.Pages?.Count > 0)
        {
            sb.AppendLine("\nPAGES (generate route files and components for each):");
            foreach (var page in spec.Pages)
            {
                sb.Append($"  - {page.Route}");
                if (!string.IsNullOrWhiteSpace(page.Name)) sb.Append($" ({page.Name})");
                if (!string.IsNullOrWhiteSpace(page.Layout)) sb.Append($" [layout: {page.Layout}]");
                sb.AppendLine();
                if (!string.IsNullOrWhiteSpace(page.Description))
                    sb.AppendLine($"    Description: {page.Description}");
                if (page.Components?.Count > 0)
                    sb.AppendLine($"    Components: {string.Join(", ", page.Components)}");
                if (page.DataRequirements?.Count > 0)
                    sb.AppendLine($"    Data Requirements: {string.Join(", ", page.DataRequirements)}");
            }
        }

        // API Routes — full detail with request/response shapes, auth flags, handlers
        if (spec.ApiRoutes?.Count > 0)
        {
            sb.AppendLine("\nAPI ROUTES (generate handler files for each with exact request/response shapes):");
            foreach (var route in spec.ApiRoutes)
            {
                sb.Append($"  - {route.Method} {route.Path}");
                if (route.Auth) sb.Append(" [AUTH REQUIRED]");
                sb.AppendLine();
                if (!string.IsNullOrWhiteSpace(route.Description))
                    sb.AppendLine($"    Description: {route.Description}");
                if (!string.IsNullOrWhiteSpace(route.Handler))
                    sb.AppendLine($"    Handler: {route.Handler}");
                if (route.RequestBody != null)
                    sb.AppendLine($"    Request Body: {System.Text.Json.JsonSerializer.Serialize(route.RequestBody)}");
                if (route.ResponseShape != null)
                    sb.AppendLine($"    Response Shape: {System.Text.Json.JsonSerializer.Serialize(route.ResponseShape)}");
            }
        }

        // Validations from the plan
        if (spec.Validations?.Count > 0)
        {
            sb.AppendLine("\nVALIDATION RULES FROM PLAN (implement these in your code):");
            foreach (var val in spec.Validations)
            {
                sb.AppendLine($"  - [{val.Category}] {val.Description} (target: {val.Target}, assertion: {val.Assertion})");
            }
        }

        // File manifest from the plan (dynamic required files derived from spec, NOT hardcoded)
        if (spec.FileManifest?.Count > 0)
        {
            sb.AppendLine("\nFILE MANIFEST FROM PLAN (these files are expected — generate all that belong to this layer):");
            foreach (var file in spec.FileManifest)
            {
                sb.Append($"  - {file.Path}");
                if (!string.IsNullOrWhiteSpace(file.Type)) sb.Append($" [{file.Type}]");
                if (!string.IsNullOrWhiteSpace(file.Description)) sb.Append($" — {file.Description}");
                sb.AppendLine();
            }
        }

        // Dependency plan
        if (spec.DependencyPlan?.Dependencies?.Count > 0)
        {
            sb.AppendLine("\nDEPENDENCIES TO ADD:");
            foreach (var dep in spec.DependencyPlan.Dependencies.Where(d => !d.IsExisting))
            {
                sb.AppendLine($"  - {dep.Name}@{dep.Version} ({dep.Purpose})");
            }
        }

        if (spec.DependencyPlan?.EnvVars?.Count > 0)
        {
            sb.AppendLine("\nREQUIRED ENVIRONMENT VARIABLES:");
            foreach (var env in spec.DependencyPlan.EnvVars)
            {
                sb.AppendLine($"  - {env.Key}: {env.Value}");
            }
        }

        // ── REQUIRED FILES CHECKLIST (prefer spec-derived, fallback to framework defaults) ──
        var resolvedRequiredFiles = ResolveRequiredFiles(requiredFiles, spec, framework);
        sb.AppendLine(@"
REQUIRED FILES CHECKLIST:
You MUST generate ALL of the following files. Each file listed here MUST appear in your ""files"" array.");
        foreach (var file in resolvedRequiredFiles)
        {
            sb.AppendLine($"  - [ ] {file}");
        }

        // ── KNOWN FAILURES ──
        if (!string.IsNullOrWhiteSpace(knownFailures))
        {
            sb.AppendLine($@"
KNOWN FAILURES FROM PREVIOUS ATTEMPTS (you MUST avoid repeating these exact patterns — study them and generate corrected code):
{knownFailures}");
        }

        // ── FEW-SHOT EXAMPLE ──
        if (!string.IsNullOrWhiteSpace(fewShotExample))
        {
            sb.AppendLine($@"
FEW-SHOT EXAMPLE (a successful generation — follow this structure):
{fewShotExample}");
        }

        // ── NON-NEGOTIABLE RULES ──
        sb.AppendLine($@"
NON-NEGOTIABLE RULES:
1. YOU ARE GENERATING THIS LAYER FROM SCRATCH (unless PREVIOUSLY GENERATED LAYERS are provided above — those are final and must not be re-created).
2. YOU MUST FOLLOW THE FOLDER STRUCTURE DEFINED IN THE APPROVED README. The README is your architectural blueprint. Do NOT invent a different folder layout.
3. Every import must resolve. Every file, component, type, route, and utility you reference must either exist in a previous layer, the scaffold baseline, or be created in your output.
4. YOUR CODE WILL BE IMMEDIATELY BUILT (npm run build / dotnet build) ON THE SERVER. Any syntax error, missing dependency in package.json, broken import, or type mismatch will fail the build and your entire output will be rejected.
5. NO TODOs, placeholders, mock data, partial snippets, or ""coming soon"" comments. Every file must contain COMPLETE, WORKING, PRODUCTION-READY code.
6. BE GENEROUS WITH CODE. Minimal skeletons or ""Hello World"" implementations are strictly forbidden. Implement:
   - Full business logic for every feature in the spec
   - Comprehensive UI components using the styling library from the stack (e.g. {stack?.Styling ?? "the chosen CSS framework"})
   - Complete data schemas with all fields, constraints, and relations from the ENTITIES section
   - Detailed error handling, loading states, empty states, and form validation
   - Real CRUD operations — not stubs
7. Use modern, production-grade patterns consistent with the stack: {framework} conventions, {stack?.Orm ?? "the chosen ORM"} for data access, {stack?.Auth ?? "the chosen auth"} for authentication.
8. If auth is required ({stack?.Auth ?? "none"}), wire the full flow end-to-end: session/token handling, protected routes, login state, middleware, and server/client boundaries.
9. If you create an API route, the frontend MUST call the exact path and match the exact request/response shape. If the frontend calls a route, that route MUST exist.
10. All files from the REQUIRED FILES CHECKLIST and the FILE MANIFEST that belong to this layer MUST appear in your output.
11. Ensure the README file is never deleted. If you modify it, preserve the agreed folder structure.
12. Optimize for a WORKING APPLICATION over cleverness or elegance.
13. NEVER use next.config.ts — Next.js does not support TypeScript config files. Always use next.config.mjs (ESM) or next.config.js (CJS).

QUALITY BAR:
- Complete file contents — never partial snippets or truncated code
- Type-safe code throughout (TypeScript strict mode, Zod validation, etc.)
- Correct imports and exports — every import resolves to a real file
- Loading, empty, success, and error states for all data-fetching UI
- Real form validation matching the VALIDATION RULES from the spec
- Accessible and responsive UI
- Sensible defaults for all configuration
- No dead code or unreachable branches
- Shared utilities and components where logic would otherwise be duplicated

DEPENDENCY POLICY:
- Prefer the scaffold's existing dependencies first — do not add duplicates
- Add the fewest extra packages necessary for the requested features
- When adding packages, choose current stable versions compatible with the scaffold
- If a dependency change is required, include the updated package.json in your output
- Every dependency you add MUST have a corresponding import somewhere in your code");

        // ── OUTPUT FORMAT ──
        sb.AppendLine(@"
OUTPUT FORMAT:
You MUST return a single valid JSON object. No markdown fences, no commentary before or after.

JSON SCHEMA:
{
  ""architecture"": ""<brief description of this layer and how it integrates with the rest of the app>"",
  ""modules"": [""<logical module names this layer introduces>""],
  ""files"": [
    {
      ""path"": ""<file path relative to project root — MUST match the folder structure from the README>"",
      ""content"": ""<COMPLETE file content — no truncation, no placeholders>""
    }
  ],
  ""requiredFiles"": [""<list of ALL file paths you generated — must include every file from REQUIRED FILES CHECKLIST>""],
  ""selfCheck"": {
    ""passed"": true|false,
    ""checks"": [
      {
        ""rule"": ""<rule id>"",
        ""passed"": true|false,
        ""notes"": ""<specific evidence: which files/imports/routes you verified>""
      }
    ]
  }
}");

        // ── MANDATORY PRE-RETURN VALIDATION PROCEDURE ──
        sb.AppendLine(@"
MANDATORY PRE-RETURN VALIDATION PROCEDURE:
Before returning your JSON, you MUST perform ALL of the following validation steps internally. This is not optional — treat it as a build step. If any check fails, you MUST fix the code in your output and re-run the check until it passes.

STEP 1 — IMPORT RESOLUTION AUDIT (rule: all-imports-resolve):
  Walk every file in your output. For each import/require statement:
    a) Check if the target file exists in your output, the scaffold baseline, the previously generated layers, or node_modules (a known npm package).
    b) If the import target does NOT exist anywhere, either create the missing file or fix the import path.
    c) Verify that named imports (e.g. { UserSchema }) match actual exports in the target file.
  In your selfCheck notes, list at least 3 specific import chains you verified (e.g. ""src/app/dashboard/page.tsx imports @/lib/db → exists in output"").

STEP 2 — COMPLETENESS AUDIT (rule: no-todos-or-placeholders):
  Scan every file for: TODO, FIXME, HACK, XXX, placeholder, ""coming soon"", ""implement later"", ""add logic here"", empty function bodies, or stub returns.
  If found, replace with real implementation.
  In your selfCheck notes, confirm ""scanned N files, zero placeholders found"".

STEP 3 — FOLDER STRUCTURE COMPLIANCE (rule: scaffold-compatibility):
  Compare every file path in your output against the folder structure defined in the APPROVED README.
  If a file is placed in a directory not described in the README, move it to the correct location or justify why it's needed.
  Verify you are using the correct framework conventions (e.g. App Router src/app/ for Next.js, NOT /pages).
  In your selfCheck notes, confirm which README sections you verified against.

STEP 4 — API CONTRACT ALIGNMENT (rule: routes-and-apis-aligned):
  For every API route in the SPECIFICATION:
    a) Verify a handler file exists in your output (or in a previous layer).
    b) If this is the frontend layer: verify every fetch/axios/API call targets an exact route path from the backend layer. Check method (GET/POST/etc), path, request body shape, and response shape all match.
    c) If this is the backend layer: verify every handler's request parsing and response shape match the spec.
  In your selfCheck notes, list each route and where its handler/consumer exists.

STEP 5 — DEPENDENCY COMPATIBILITY (rule: dependencies-compatible):
  Check that every package in your generated package.json is compatible with the framework version in the scaffold.
  Verify no duplicate packages or version conflicts.
  Verify every added package is actually imported somewhere in your code.
  In your selfCheck notes, list any packages you added and why.

STEP 6 — SCHEMA CONSISTENCY (rule: schema-consistent):
  Verify that your schema/models match the ENTITIES section of the spec EXACTLY:
    a) Every entity from the spec has a corresponding model
    b) Every field has the correct type, constraints (required, unique, maxLength), and defaults
    c) Every relation is correctly defined with proper foreign keys
    d) Field names in queries, API handlers, and frontend forms match the schema field names exactly
  In your selfCheck notes, list each entity and confirm field count matches.

STEP 7 — AUTH END-TO-END (rule: auth-wired-end-to-end):
  If auth is required by the stack config:
    a) Verify login/register pages or components exist
    b) Verify session/token middleware is wired
    c) Verify protected routes check auth state
    d) Verify API routes that require auth have middleware/guards
  If auth is NOT required, mark as passed with ""auth not required by stack config"".

STEP 8 — ENVIRONMENT VARIABLES (rule: env-vars-declared):
  Collect every process.env.* or env.* reference across all generated files.
  Verify each one appears in .env.example with a description.
  In your selfCheck notes, list the env vars you found and confirmed.

CRITICAL: After running all 8 steps, if ANY check has ""passed"": false, you MUST go back and fix the failing code in your files array BEFORE returning. Do NOT return code that you know fails validation. The selfCheck must be an accurate reflection of your FINAL code, not a pre-fix assessment.");

        return sb.ToString();
    }

    /// <summary>
    /// Builds the user prompt for a layer generation request.
    /// </summary>
    public static string BuildLayerUserPrompt(
        string userInstruction,
        StringBuilder context,
        string originalPrompt,
        string approvedReadme)
    {
        return $@"Instruction: {userInstruction}
Original Requirement: {originalPrompt}
Context: {context}

Generate the code for this layer. Return the complete JSON object matching the schema defined in the system prompt.";
    }

    /// <summary>
    /// Resolves the required files list. Priority:
    /// 1. Explicitly provided requiredFiles
    /// 2. FileManifest from the approved spec (dynamic, plan-derived)
    /// 3. Framework defaults (fallback)
    /// </summary>
    private static List<string> ResolveRequiredFiles(List<string> requiredFiles, string framework)
    {
        return ResolveRequiredFiles(requiredFiles, null, framework);
    }

    private static List<string> ResolveRequiredFiles(List<string> requiredFiles, AppSpecDto spec, string framework)
    {
        if (requiredFiles != null && requiredFiles.Count > 0)
            return requiredFiles;

        // Derive from spec's FileManifest if available
        if (spec?.FileManifest?.Count > 0)
        {
            var manifestPaths = spec.FileManifest
                .Where(f => !string.IsNullOrWhiteSpace(f.Path))
                .Select(f => f.Path)
                .ToList();
            if (manifestPaths.Count > 0)
                return manifestPaths;
        }

        var lower = framework?.ToLowerInvariant() ?? "";
        if (lower.Contains("vite") || lower.Contains("react"))
            return DefaultViteRequiredFiles;

        return DefaultNextJsRequiredFiles;
    }
}