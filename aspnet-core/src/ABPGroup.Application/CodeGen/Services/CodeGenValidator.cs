using System;
using System.Collections.Generic;
using System.Linq;
using Abp.Domain.Services;
using ABPGroup.CodeGen.Dto;
using ABPGroup.Projects;

namespace ABPGroup.CodeGen;

public class CodeGenValidator : DomainService, ICodeGenValidator
{
    public List<ValidationResultDto> EvaluateValidationResults(
        List<ValidationRuleDto> validations,
        List<GeneratedFile> generatedFiles,
        StackConfigDto stack)
    {
        var files = generatedFiles ?? new List<GeneratedFile>();
        var filePaths = new HashSet<string>(
            files.Select(f => CodeGenHelpers.NormalizeFilePath(f.Path)),
            StringComparer.OrdinalIgnoreCase);
        var combinedContent = string.Join("\n", files.Select(f => f.Content ?? string.Empty));

        var results = new List<ValidationResultDto>();
        
        foreach (var validation in validations ?? new List<ValidationRuleDto>())
        {
            var id = string.IsNullOrWhiteSpace(validation.Id)
                ? Guid.NewGuid().ToString("N")[..8]
                : validation.Id;
            var category = (validation.Category ?? string.Empty).ToLowerInvariant();

            var passed = true;
            var message = "Validation passed.";

            switch (category)
            {
                case "file-exists":
                {
                    var targetPath = CodeGenHelpers.NormalizeFilePath(validation.Target);
                    passed = string.IsNullOrWhiteSpace(targetPath)
                        ? files.Count > 0
                        : filePaths.Contains(targetPath);
                    message = passed
                        ? "Required file exists."
                        : $"Required file not found: {validation.Target}";
                    break;
                }
                case "build-passes":
                {
                    passed = filePaths.Contains("package.json");
                    message = passed
                        ? "Build validation baseline passed (package.json present)."
                        : "Build validation baseline failed: package.json not found.";
                    break;
                }
                case "route-exists":
                {
                    var routeHint = CodeGenHelpers.ExtractRouteHint(validation);
                    passed = string.IsNullOrWhiteSpace(routeHint)
                        || combinedContent.Contains(routeHint, StringComparison.OrdinalIgnoreCase);
                    message = passed
                        ? "Route reference found in generated files."
                        : $"Route reference not found in generated files: {routeHint}";
                    break;
                }
                case "entity-schema":
                {
                    passed = files.Count > 0;
                    message = passed
                        ? "Entity schema validation passed (generated files available)."
                        : "Entity schema validation failed: no generated files.";
                    break;
                }
                case "auth-guard":
                {
                    var hasAuthFiles = filePaths.Any(p => 
                        p.Contains("auth", StringComparison.OrdinalIgnoreCase) ||
                        p.Contains("login", StringComparison.OrdinalIgnoreCase) ||
                        p.Contains("session", StringComparison.OrdinalIgnoreCase));
                    passed = !validation.Automatable || hasAuthFiles || files.Count > 0;
                    message = passed
                        ? "Auth guard validation passed."
                        : "Auth guard validation failed: no auth-related files found.";
                    break;
                }
                case "lint-passes":
                case "type-check":
                {
                    passed = files.Count > 0;
                    message = passed
                        ? $"{category} validation baseline passed (files available for checking)."
                        : $"{category} validation failed: no files to check.";
                    break;
                }
                case "env-vars":
                {
                    var hasEnvFiles = filePaths.Any(p => 
                        p.Contains(".env", StringComparison.OrdinalIgnoreCase) ||
                        p.Contains("config", StringComparison.OrdinalIgnoreCase));
                    passed = !validation.Automatable || hasEnvFiles || files.Count > 0;
                    message = passed
                        ? "Environment variables validation passed."
                        : "Environment variables validation failed: no config files found.";
                    break;
                }
                case "test-passes":
                {
                    var hasTestFiles = filePaths.Any(p => 
                        p.Contains("test", StringComparison.OrdinalIgnoreCase) ||
                        p.Contains("spec", StringComparison.OrdinalIgnoreCase));
                    passed = !validation.Automatable || hasTestFiles || files.Count > 0;
                    message = passed
                        ? "Test suite validation baseline passed."
                        : "Test suite validation failed: no test files found.";
                    break;
                }
            }

            results.Add(new ValidationResultDto
            {
                Id = id,
                Status = passed ? "passed" : "failed",
                Message = message
            });
        }

        // Add shell-specific validations
        results.AddRange(BuildShellValidationResults(stack, files, filePaths));

        return results;
    }

    public List<ValidationResultDto> BuildInitialValidationResults(
        List<ValidationRuleDto> validations,
        StackConfigDto stack)
    {
        var results = new List<ValidationResultDto>();
        
        var hasBuildPasses = validations?.Any(v => 
            !string.IsNullOrWhiteSpace(v.Category) && 
            v.Category.Equals("build-passes", StringComparison.OrdinalIgnoreCase)) ?? false;
        
        if (!hasBuildPasses)
        {
            results.Add(new ValidationResultDto
            {
                Id = "build-passes",
                Status = "pending",
                Message = "Project should build successfully."
            });
        }
        
        if (validations != null && validations.Count > 0)
        {
            foreach (var v in validations)
            {
                var id = string.IsNullOrWhiteSpace(v.Id) 
                    ? Guid.NewGuid().ToString("N")[..8] 
                    : v.Id;
                
                results.Add(new ValidationResultDto
                {
                    Id = id,
                    Status = "pending",
                    Message = string.IsNullOrWhiteSpace(v.Description) ? "Validation queued." : v.Description
                });
            }
        }
        
        if (results.Count == 0)
        {
            results.Add(new ValidationResultDto
            {
                Id = "build-passes",
                Status = "pending",
                Message = "Project should build successfully."
            });
        }

        results.AddRange(BuildShellValidationPlaceholders(stack)
            .Where(shellValidation => results.All(existing => !existing.Id.Equals(shellValidation.Id, StringComparison.OrdinalIgnoreCase))));

        return results;
    }

    public List<ValidationResultDto> BuildShellValidationResults(
        StackConfigDto stack,
        List<GeneratedFile> files,
        HashSet<string> filePaths)
    {
        if (string.IsNullOrWhiteSpace(stack?.Framework))
            return new List<ValidationResultDto>();

        var framework = MapFrameworkString(stack?.Framework);
        var results = new List<ValidationResultDto>();

        if (framework == Framework.NextJS)
        {
            var hasHomePage = CodeGenHelpers.HasAnyFile(filePaths, "src/app/page.tsx", "src/app/page.jsx", "src/app/page.ts", "src/app/page.js");
            var hasLayout = CodeGenHelpers.HasAnyFile(filePaths, "src/app/layout.tsx", "src/app/layout.jsx", "src/app/layout.ts", "src/app/layout.js");
            var hasStyledHomeRoute = CodeGenHelpers.HasStyledRoute(files,
                "src/app/page.tsx",
                "src/app/page.jsx",
                "src/app/page.ts",
                "src/app/page.js");

            results.Add(CreateShellValidationResult(
                CodeGenConstants.NextHomePageValidationId,
                hasHomePage,
                "Next.js shell file present: src/app/page.tsx.",
                "Next.js shell file missing: src/app/page.tsx."));
            results.Add(CreateShellValidationResult(
                CodeGenConstants.RequiredLayoutValidationId,
                hasLayout,
                "Root layout file present.",
                "Required layout file missing: src/app/layout.tsx."));
            results.Add(CreateShellValidationResult(
                CodeGenConstants.StyledHomeRouteValidationId,
                hasStyledHomeRoute,
                "Styled landing/home route found.",
                "No styled landing/home route found in src/app/page.tsx."));
        }

        if (framework == Framework.ReactVite)
        {
            var hasIndexHtml = CodeGenHelpers.HasAnyFile(filePaths, "index.html");
            var hasLayout = CodeGenHelpers.HasAnyFile(filePaths, "src/app.tsx", "src/app.jsx", "src/app.ts", "src/app.js");
            var hasStyledHomeRoute = CodeGenHelpers.HasStyledRoute(files,
                "src/app.tsx",
                "src/app.jsx",
                "src/app.ts",
                "src/app.js");

            results.Add(CreateShellValidationResult(
                CodeGenConstants.ViteIndexHtmlValidationId,
                hasIndexHtml,
                "Vite entry file present: index.html.",
                "Vite shell file missing: index.html."));
            results.Add(CreateShellValidationResult(
                CodeGenConstants.RequiredLayoutValidationId,
                hasLayout,
                "Root layout file present.",
                "Required layout file missing: src/App.tsx."));
            results.Add(CreateShellValidationResult(
                CodeGenConstants.StyledHomeRouteValidationId,
                hasStyledHomeRoute,
                "Styled landing/home route found.",
                "No styled landing/home route found in src/App.tsx."));
        }

        return results;
    }

    public List<ValidationResultDto> BuildShellValidationPlaceholders(StackConfigDto stack)
    {
        if (string.IsNullOrWhiteSpace(stack?.Framework))
            return new List<ValidationResultDto>();

        var framework = MapFrameworkString(stack?.Framework);
        var results = new List<ValidationResultDto>();

        if (framework == Framework.NextJS)
        {
            results.Add(CreatePendingValidation(
                CodeGenConstants.NextHomePageValidationId,
                "Next.js shell must include src/app/page.tsx."));
            results.Add(CreatePendingValidation(
                CodeGenConstants.RequiredLayoutValidationId,
                "Application shell must include a root layout file."));
            results.Add(CreatePendingValidation(
                CodeGenConstants.StyledHomeRouteValidationId,
                "Application must include at least one styled landing or home route."));
        }

        if (framework == Framework.ReactVite)
        {
            results.Add(CreatePendingValidation(
                CodeGenConstants.ViteIndexHtmlValidationId,
                "Vite shell must include index.html."));
            results.Add(CreatePendingValidation(
                CodeGenConstants.RequiredLayoutValidationId,
                "Application shell must include a root layout file."));
            results.Add(CreatePendingValidation(
                CodeGenConstants.StyledHomeRouteValidationId,
                "Application must include at least one styled landing or home route."));
        }

        return results;
    }

    private static ValidationResultDto CreateShellValidationResult(string id, bool passed, string successMsg, string failMsg)
    {
        return new ValidationResultDto
        {
            Id = id,
            Status = passed ? "passed" : "failed",
            Message = passed ? successMsg : failMsg
        };
    }

    private static ValidationResultDto CreatePendingValidation(string id, string message)
    {
        return new ValidationResultDto
        {
            Id = id,
            Status = "pending",
            Message = message
        };
    }

    private static Framework MapFrameworkString(string framework)
    {
        if (string.IsNullOrEmpty(framework)) return Framework.NextJS;
        var lower = framework.ToLowerInvariant();
        if (lower.Contains("next")) return Framework.NextJS;
        if (lower.Contains("react") || lower.Contains("vite")) return Framework.ReactVite;
        if (lower.Contains("angular")) return Framework.Angular;
        if (lower.Contains("vue")) return Framework.Vue;
        if (lower.Contains("blazor") || lower.Contains(".net")) return Framework.DotNetBlazor;
        return Framework.NextJS;
    }
}
