using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ABPGroup.CodeGen.Dto;
using ABPGroup.Projects;

namespace ABPGroup.CodeGen;

public static class CodeGenHelpers
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public static string ParseDelimitedSection(string content, string sectionName)
    {
        if (string.IsNullOrEmpty(content)) return null;

        var startTag = $"==={sectionName}===";
        var endTag = $"===END {sectionName}===";

        var startIdx = content.IndexOf(startTag, StringComparison.OrdinalIgnoreCase);
        if (startIdx < 0) return null;

        startIdx += startTag.Length;
        var endIdx = content.IndexOf(endTag, startIdx, StringComparison.OrdinalIgnoreCase);
        if (endIdx < 0) return content[startIdx..].Trim();

        return content[startIdx..endIdx].Trim();
    }

    public static List<GeneratedFileDto> ParseFiles(string content)
    {
        var files = new List<GeneratedFileDto>();
        if (string.IsNullOrEmpty(content)) return files;

        var remaining = content;
        while (true)
        {
            var fileStart = remaining.IndexOf("===FILE===", StringComparison.OrdinalIgnoreCase);
            if (fileStart < 0) break;

            var pathStart = fileStart + "===FILE===".Length;
            var contentTag = remaining.IndexOf("===CONTENT===", pathStart, StringComparison.OrdinalIgnoreCase);
            if (contentTag < 0) break;

            var path = remaining[pathStart..contentTag].Trim();
            var contentStart = contentTag + "===CONTENT===".Length;
            var fileEnd = remaining.IndexOf("===END FILE===", contentStart, StringComparison.OrdinalIgnoreCase);

            string fileContent;
            if (fileEnd >= 0)
            {
                fileContent = remaining[contentStart..fileEnd].Trim();
                remaining = remaining[(fileEnd + "===END FILE===".Length)..];
            }
            else
            {
                fileContent = remaining[contentStart..].Trim();
                break;
            }

            if (!string.IsNullOrEmpty(path))
            {
                files.Add(new GeneratedFileDto { Path = path, Content = fileContent });
            }
        }

        return files;
    }

    public static List<string> ParseModules(string content)
    {
        var modulesStr = ParseDelimitedSection(content, "MODULES");
        return ParseCsvList(modulesStr);
    }

    public static List<string> ParseCsvList(string csv)
    {
        if (string.IsNullOrWhiteSpace(csv)) return new List<string>();
        return csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();
    }

    public static AppSpecDto ParseSpecOrDefault(string specJson, out string warning)
    {
        warning = null;
        if (string.IsNullOrWhiteSpace(specJson))
            return NormalizeSpec(new AppSpecDto());

        try
        {
            var strict = JsonSerializer.Deserialize<AppSpecDto>(specJson, JsonOptions) ?? new AppSpecDto();
            return NormalizeSpec(strict);
        }
        catch (Exception strictEx)
        {
            try
            {
                using var doc = JsonDocument.Parse(specJson);
                var fallback = BuildSpecFromJson(doc.RootElement);
                warning = $"GenerateSpec: Strict parse failed ({strictEx.Message}). Tolerant parser was used.";
                return NormalizeSpec(fallback);
            }
            catch (Exception fallbackEx)
            {
                warning = $"GenerateSpec: Failed to parse spec JSON: {fallbackEx.Message}. Raw: {specJson[..Math.Min(specJson.Length, 200)]}";
                return NormalizeSpec(new AppSpecDto());
            }
        }
    }

    private static AppSpecDto BuildSpecFromJson(JsonElement root)
    {
        return new AppSpecDto
        {
            Entities = ParseEntities(GetPropertyCaseInsensitive(root, "entities")),
            Pages = ParsePages(GetPropertyCaseInsensitive(root, "pages")),
            ApiRoutes = ParseApiRoutes(GetPropertyCaseInsensitive(root, "apiRoutes")),
            Validations = ParseValidations(GetPropertyCaseInsensitive(root, "validations")),
            FileManifest = ParseFileManifest(GetPropertyCaseInsensitive(root, "fileManifest"))
        };
    }

    private static List<EntitySpecDto> ParseEntities(JsonElement section)
    {
        var items = new List<EntitySpecDto>();
        foreach (var element in EnumerateSection(section))
        {
            if (TryDeserialize(element, out EntitySpecDto entity))
            {
                entity.Fields ??= new List<FieldSpecDto>();
                entity.Relations ??= new List<RelationSpecDto>();
                if (string.IsNullOrWhiteSpace(entity.TableName) && !string.IsNullOrWhiteSpace(entity.Name))
                    entity.TableName = entity.Name.ToLowerInvariant();
                items.Add(entity);
                continue;
            }

            if (element.ValueKind == JsonValueKind.String)
            {
                var name = element.GetString();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    items.Add(new EntitySpecDto
                    {
                        Name = name,
                        TableName = name.ToLowerInvariant(),
                        Fields = new List<FieldSpecDto>(),
                        Relations = new List<RelationSpecDto>()
                    });
                }
            }
        }

        return items;
    }

    private static List<PageSpecDto> ParsePages(JsonElement section)
    {
        var items = new List<PageSpecDto>();
        foreach (var element in EnumerateSection(section))
        {
            if (TryDeserialize(element, out PageSpecDto page))
            {
                page.Components ??= new List<string>();
                page.DataRequirements ??= new List<string>();
                items.Add(page);
                continue;
            }

            if (element.ValueKind == JsonValueKind.String)
            {
                var route = element.GetString();
                if (!string.IsNullOrWhiteSpace(route))
                {
                    items.Add(new PageSpecDto
                    {
                        Route = route,
                        Name = route.Trim('/'),
                        Layout = "authenticated",
                        Components = new List<string>(),
                        DataRequirements = new List<string>(),
                        Description = string.Empty
                    });
                }
            }

            if (element.ValueKind == JsonValueKind.Object)
            {
                var route = GetStringProperty(element, "route")
                    ?? GetStringProperty(element, "path")
                    ?? GetStringProperty(element, "url");

                if (!string.IsNullOrWhiteSpace(route))
                {
                    var components = ParseStringList(GetPropertyCaseInsensitive(element, "components"));
                    var dataRequirements = ParseStringList(GetPropertyCaseInsensitive(element, "dataRequirements"));
                    var layout = (GetStringProperty(element, "layout") ?? "authenticated").ToLowerInvariant();
                    if (layout is not ("authenticated" or "public" or "admin"))
                        layout = "authenticated";

                    items.Add(new PageSpecDto
                    {
                        Route = NormalizePageRoute(route),
                        Name = GetStringProperty(element, "name") ?? BuildPageNameFromRoute(route),
                        Layout = layout,
                        Components = components,
                        DataRequirements = dataRequirements,
                        Description = GetStringProperty(element, "description") ?? string.Empty
                    });
                }
            }
        }

        return items;
    }

    private static List<ApiRouteSpecDto> ParseApiRoutes(JsonElement section)
    {
        var items = new List<ApiRouteSpecDto>();
        foreach (var element in EnumerateSection(section))
        {
            if (TryDeserialize(element, out ApiRouteSpecDto route))
            {
                route.ResponseShape ??= new { };
                items.Add(route);
                continue;
            }

            if (element.ValueKind == JsonValueKind.Object)
            {
                var method = GetStringProperty(element, "method") ?? "GET";
                var path = GetStringProperty(element, "path") ?? string.Empty;
                var handler = GetStringProperty(element, "handler") ?? string.Empty;
                var description = GetStringProperty(element, "description") ?? string.Empty;

                items.Add(new ApiRouteSpecDto
                {
                    Method = method,
                    Path = path,
                    Handler = handler,
                    RequestBody = ToLooseObject(GetPropertyCaseInsensitive(element, "requestBody")),
                    ResponseShape = ToLooseObject(GetPropertyCaseInsensitive(element, "responseShape")) ?? new { },
                    Auth = GetBoolProperty(element, "auth"),
                    Description = description
                });
            }
        }

        return items;
    }

    private static List<ValidationRuleDto> ParseValidations(JsonElement section)
    {
        var items = new List<ValidationRuleDto>();
        foreach (var element in EnumerateSection(section))
        {
            if (TryDeserialize(element, out ValidationRuleDto validation))
            {
                items.Add(validation);
                continue;
            }

            if (element.ValueKind == JsonValueKind.String)
            {
                var description = element.GetString();
                if (!string.IsNullOrWhiteSpace(description))
                {
                    var id = Slugify(description);
                    items.Add(new ValidationRuleDto
                    {
                        Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N")[..8] : id,
                        Category = "build-passes",
                        Description = description,
                        Target = "project",
                        Assertion = description,
                        Automatable = false
                    });
                }
                continue;
            }

            if (element.ValueKind == JsonValueKind.Object)
            {
                var description = GetStringProperty(element, "description")
                    ?? GetStringProperty(element, "assertion")
                    ?? GetStringProperty(element, "name")
                    ?? "Validation rule";

                var category = (GetStringProperty(element, "category") ?? "build-passes").ToLowerInvariant();
                var id = GetStringProperty(element, "id") ?? Slugify(description);
                items.Add(new ValidationRuleDto
                {
                    Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N")[..8] : id,
                    Category = category,
                    Description = description,
                    Target = GetStringProperty(element, "target") ?? "project",
                    Assertion = GetStringProperty(element, "assertion") ?? description,
                    Automatable = GetBoolProperty(element, "automatable"),
                    Script = GetStringProperty(element, "script")
                });
            }
        }

        return items;
    }

    private static List<FileEntryDto> ParseFileManifest(JsonElement section)
    {
        var items = new List<FileEntryDto>();
        foreach (var element in EnumerateSection(section))
        {
            if (TryDeserialize(element, out FileEntryDto file))
            {
                items.Add(file);
                continue;
            }

            if (element.ValueKind == JsonValueKind.String)
            {
                var path = element.GetString();
                if (!string.IsNullOrWhiteSpace(path))
                {
                    items.Add(new FileEntryDto
                    {
                        Path = path,
                        Type = "generated",
                        Description = string.Empty
                    });
                }
            }
        }

        return items;
    }

    private static IEnumerable<JsonElement> EnumerateSection(JsonElement section)
    {
        if (section.ValueKind == JsonValueKind.Array)
            return section.EnumerateArray();

        if (section.ValueKind is JsonValueKind.Object or JsonValueKind.String)
            return new[] { section };

        return Enumerable.Empty<JsonElement>();
    }

    private static bool TryDeserialize<T>(JsonElement element, out T result) where T : class
    {
        try
        {
            result = JsonSerializer.Deserialize<T>(element.GetRawText(), JsonOptions);
            return result != null;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    private static JsonElement GetPropertyCaseInsensitive(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return default;

        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                return property.Value;
        }

        return default;
    }

    private static string GetStringProperty(JsonElement element, string propertyName)
    {
        var prop = GetPropertyCaseInsensitive(element, propertyName);
        if (prop.ValueKind == JsonValueKind.String)
            return prop.GetString();

        if (prop.ValueKind is JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False)
            return prop.GetRawText();

        return null;
    }

    private static bool GetBoolProperty(JsonElement element, string propertyName)
    {
        var prop = GetPropertyCaseInsensitive(element, propertyName);
        return prop.ValueKind == JsonValueKind.True ||
               (prop.ValueKind == JsonValueKind.String && bool.TryParse(prop.GetString(), out var parsed) && parsed);
    }

    private static object ToLooseObject(JsonElement element)
    {
        if (element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            return null;

        if (element.ValueKind == JsonValueKind.String)
            return element.GetString();

        if (element.ValueKind == JsonValueKind.Number)
        {
            if (element.TryGetInt64(out var asInt)) return asInt;
            if (element.TryGetDouble(out var asDouble)) return asDouble;
        }

        if (element.ValueKind == JsonValueKind.True || element.ValueKind == JsonValueKind.False)
            return element.GetBoolean();

        try
        {
            return JsonSerializer.Deserialize<object>(element.GetRawText(), JsonOptions);
        }
        catch
        {
            return element.GetRawText();
        }
    }

    public static AppSpecDto NormalizeSpec(AppSpecDto spec)
    {
        spec ??= new AppSpecDto();

        spec.Entities ??= new List<EntitySpecDto>();
        spec.Pages ??= new List<PageSpecDto>();
        spec.ApiRoutes ??= new List<ApiRouteSpecDto>();
        spec.Validations ??= new List<ValidationRuleDto>();
        spec.FileManifest ??= new List<FileEntryDto>();

        spec.Entities = spec.Entities
            .Where(e => e != null)
            .Select(e =>
            {
                e.Fields ??= new List<FieldSpecDto>();
                e.Relations ??= new List<RelationSpecDto>();
                if (string.IsNullOrWhiteSpace(e.TableName) && !string.IsNullOrWhiteSpace(e.Name))
                    e.TableName = e.Name.ToLowerInvariant();
                return e;
            })
            .Where(e => !string.IsNullOrWhiteSpace(e.Name))
            .GroupBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        spec.ApiRoutes = spec.ApiRoutes
            .Where(r => r != null)
            .Select(r =>
            {
                r.Method = string.IsNullOrWhiteSpace(r.Method) ? "GET" : r.Method.ToUpperInvariant();
                r.Path = NormalizeApiPath(r.Path);
                r.ResponseShape ??= new { };
                r.Description ??= string.Empty;
                return r;
            })
            .Where(r => !string.IsNullOrWhiteSpace(r.Path))
            .GroupBy(r => $"{r.Method}:{r.Path}", StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        spec.FileManifest = spec.FileManifest
            .Where(f => f != null && !string.IsNullOrWhiteSpace(f.Path))
            .GroupBy(f => f.Path, StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var first = g.First();
                first.Type ??= "generated";
                first.Description ??= string.Empty;
                return first;
            })
            .ToList();

        spec.Pages = spec.Pages
            .Where(p => p != null)
            .Select(p =>
            {
                p.Route = NormalizePageRoute(p.Route);
                p.Name = string.IsNullOrWhiteSpace(p.Name) ? BuildPageNameFromRoute(p.Route) : p.Name;
                p.Layout = NormalizeLayout(p.Layout);
                p.Components ??= new List<string>();
                p.DataRequirements ??= new List<string>();
                p.Description ??= string.Empty;
                return p;
            })
            .Where(p => !string.IsNullOrWhiteSpace(p.Route))
            .GroupBy(p => p.Route, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        if (spec.Pages.Count == 0)
            spec.Pages = BuildFallbackPages(spec.ApiRoutes, spec.FileManifest);

        spec.Validations = spec.Validations
            .Where(v => v != null)
            .Select(v =>
            {
                v.Id = string.IsNullOrWhiteSpace(v.Id) ? Guid.NewGuid().ToString("N")[..8] : v.Id;
                v.Category = NormalizeValidationCategory(v.Category);
                v.Description ??= "Validation rule";
                v.Target ??= "project";
                v.Assertion ??= v.Description;
                return v;
            })
            .GroupBy(v => v.Id, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        if (spec.Validations.Count == 0)
            spec.Validations = BuildFallbackValidations(spec);

        return spec;
    }

    public static List<PageSpecDto> BuildFallbackPages(List<ApiRouteSpecDto> apiRoutes, List<FileEntryDto> fileManifest)
    {
        var routes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var apiRoute in apiRoutes ?? new List<ApiRouteSpecDto>())
        {
            var pageRoute = ApiPathToPageRoute(apiRoute.Path);
            if (!string.IsNullOrWhiteSpace(pageRoute))
                routes.Add(pageRoute);
        }

        foreach (var file in fileManifest ?? new List<FileEntryDto>())
        {
            var fromFile = FilePathToPageRoute(file.Path);
            if (!string.IsNullOrWhiteSpace(fromFile))
                routes.Add(fromFile);
        }

        if (routes.Count == 0)
            routes.Add("/");

        return routes
            .OrderBy(r => r, StringComparer.OrdinalIgnoreCase)
            .Select(route => new PageSpecDto
            {
                Route = route,
                Name = BuildPageNameFromRoute(route),
                Layout = route.StartsWith("/auth", StringComparison.OrdinalIgnoreCase)
                    || route.StartsWith("/login", StringComparison.OrdinalIgnoreCase)
                    || route.StartsWith("/register", StringComparison.OrdinalIgnoreCase)
                    ? "public"
                    : "authenticated",
                Components = new List<string>(),
                DataRequirements = new List<string>(),
                Description = "Generated fallback page from available routes/files."
            })
            .ToList();
    }

    public static string NormalizePageRoute(string route)
    {
        if (string.IsNullOrWhiteSpace(route))
            return string.Empty;

        var normalized = route.Trim();
        if (!normalized.StartsWith('/'))
            normalized = "/" + normalized;

        normalized = normalized.Replace("//", "/");
        return normalized.Length > 1 ? normalized.TrimEnd('/') : normalized;
    }

    public static string NormalizeApiPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        var normalized = path.Trim();
        if (!normalized.StartsWith('/'))
            normalized = "/" + normalized;

        normalized = normalized.Replace("//", "/");
        return normalized;
    }

    public static string ApiPathToPageRoute(string apiPath)
    {
        if (string.IsNullOrWhiteSpace(apiPath))
            return null;

        var normalized = NormalizeApiPath(apiPath);
        if (!normalized.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
            return null;

        var pageRoute = normalized[4..];
        if (string.IsNullOrWhiteSpace(pageRoute))
            return "/";

        pageRoute = Regex.Replace(pageRoute, @":([A-Za-z0-9_]+)", "[$1]");
        pageRoute = Regex.Replace(pageRoute, @"\{([A-Za-z0-9_]+)\}", "[$1]");
        return NormalizePageRoute(pageRoute);
    }

    public static string FilePathToPageRoute(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        var normalized = path.Replace('\\', '/');
        if (Regex.IsMatch(normalized, @"(^|/)app/page\.[^/]+$", RegexOptions.IgnoreCase))
            return "/";

        var match = Regex.Match(normalized, @"(^|/)app/(?<route>.+)/page\.[^/]+$", RegexOptions.IgnoreCase);
        if (!match.Success)
            return null;

        return NormalizePageRoute(match.Groups["route"].Value);
    }

    public static string BuildPageNameFromRoute(string route)
    {
        if (string.IsNullOrWhiteSpace(route) || route == "/")
            return "Home";

        var parts = route
            .Trim('/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim('[', ']'))
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(ToPascalCase)
            .ToList();

        return parts.Count == 0 ? "Page" : string.Join(string.Empty, parts);
    }

    public static string ToPascalCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var tokens = Regex.Split(value, "[^A-Za-z0-9]+")
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => char.ToUpperInvariant(t[0]) + t[1..].ToLowerInvariant());

        return string.Join(string.Empty, tokens);
    }

    public static string NormalizeLayout(string layout)
    {
        var normalized = (layout ?? "authenticated").ToLowerInvariant();
        return normalized is "authenticated" or "public" or "admin" ? normalized : "authenticated";
    }

    public static string NormalizeValidationCategory(string category)
    {
        var normalized = (category ?? "build-passes").ToLowerInvariant();
        return normalized switch
        {
            "file-exists" or "entity-schema" or "route-exists" or "build-passes" or "lint-passes" or
            "env-vars" or "test-passes" or "auth-guard" or "type-check" or "api-returns" => normalized,
            _ => "build-passes"
        };
    }

    public static List<string> ParseStringList(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            return element.EnumerateArray()
                .Where(v => v.ValueKind == JsonValueKind.String)
                .Select(v => v.GetString())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToList();
        }

        if (element.ValueKind == JsonValueKind.String)
        {
            var value = element.GetString();
            return string.IsNullOrWhiteSpace(value)
                ? new List<string>()
                : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        }

        return new List<string>();
    }

    public static string Slugify(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var slug = Regex.Replace(value.ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
        return slug.Length > 64 ? slug[..64] : slug;
    }

    public static string ToPascalIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var parts = Regex.Split(value.Trim(), @"[^A-Za-z0-9]+")
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .ToList();

        if (parts.Count == 0)
            return string.Empty;

        return string.Concat(parts.Select(part =>
            part.Length == 1
                ? part.ToUpperInvariant()
                : char.ToUpperInvariant(part[0]) + part[1..].ToLowerInvariant()));
    }

    public static string ExtractLeadingIdentifier(string rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return string.Empty;

        var stripped = rawValue.Trim().Trim('`');
        var value = stripped.Split(':', 2, StringSplitOptions.TrimEntries)[0];
        return ToPascalIdentifier(value);
    }

    public static List<string> ExtractMarkdownBulletItems(string markdown, params string[] sectionHints)
    {
        var items = new List<string>();
        if (string.IsNullOrWhiteSpace(markdown))
            return items;

        var lines = markdown.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var inSection = false;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.StartsWith("#", StringComparison.Ordinal))
            {
                inSection = sectionHints.Any(hint =>
                    line.Contains(hint, StringComparison.OrdinalIgnoreCase));
                continue;
            }

            if (!inSection || line.Length < 2 || (line[0] != '-' && line[0] != '*'))
                continue;

            items.Add(line[1..].Trim());
        }

        return items;
    }

    public static IEnumerable<string> ExtractFilePathsFromText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            yield break;

        var matches = Regex.Matches(
            text,
            @"(?<![A-Za-z0-9])(?:\.{0,2}/)?[A-Za-z0-9_\-/]+\.[A-Za-z0-9._-]+",
            RegexOptions.CultureInvariant);

        foreach (Match match in matches)
        {
            var path = NormalizeFilePath(match.Value);
            if (!string.IsNullOrWhiteSpace(path))
                yield return path;
        }
    }

    public static bool LooksLikeFilePath(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var normalized = NormalizeFilePath(value);
        return normalized.Contains('/')
            || Regex.IsMatch(normalized, @"^[A-Za-z0-9_.-]+\.[A-Za-z0-9._-]+$", RegexOptions.CultureInvariant);
    }

    public static string BuildHandlerName(string method, string path)
    {
        var suffix = string.Concat(
            Regex.Split(path ?? string.Empty, @"[^A-Za-z0-9]+")
                .Where(segment => !string.IsNullOrWhiteSpace(segment) && !segment.Equals("api", StringComparison.OrdinalIgnoreCase))
                .Select(segment => char.ToUpperInvariant(segment[0]) + segment[1..]));

        if (string.IsNullOrWhiteSpace(suffix))
            suffix = "Route";

        return $"{method.ToLowerInvariant()}{suffix}";
    }

    public static EntitySpecDto BuildFallbackEntitySpec(string entityName)
    {
        var normalizedName = ToPascalIdentifier(entityName);
        return new EntitySpecDto
        {
            Name = normalizedName,
            TableName = normalizedName.ToLowerInvariant(),
            Fields = BuildFallbackEntityFields(normalizedName),
            Relations = new List<RelationSpecDto>()
        };
    }

    public static List<FieldSpecDto> BuildFallbackEntityFields(string entityName)
    {
        var fields = new List<FieldSpecDto>
        {
            new()
            {
                Name = "id",
                Type = "string",
                Required = true,
                Unique = true,
                Description = "Stable identifier."
            }
        };

        if (entityName.Contains("Task", StringComparison.OrdinalIgnoreCase)
            || entityName.Contains("Todo", StringComparison.OrdinalIgnoreCase))
        {
            fields.Add(new FieldSpecDto
            {
                Name = "title",
                Type = "string",
                Required = true,
                MaxLength = 255,
                Description = "Short task title."
            });
            fields.Add(new FieldSpecDto
            {
                Name = "completed",
                Type = "boolean",
                Required = true,
                Description = "Whether the task is complete."
            });
            return fields;
        }

        fields.Add(new FieldSpecDto
        {
            Name = "name",
            Type = "string",
            Required = true,
            MaxLength = 255,
            Description = $"{entityName} display name."
        });

        return fields;
    }

    public static List<ValidationRuleDto> BuildFallbackValidations(AppSpecDto spec)
    {
        var rules = new List<ValidationRuleDto>
        {
            new()
            {
                Id = "build-passes",
                Category = "build-passes",
                Description = "Project should build successfully.",
                Target = "project",
                Assertion = "Build command exits with status code 0.",
                Automatable = true,
                Script = "npm run build"
            }
        };

        if (spec.Entities.Any())
        {
            rules.Add(new ValidationRuleDto
            {
                Id = "entity-schema",
                Category = "entity-schema",
                Description = "Entity definitions should include required identifiers and key fields.",
                Target = "entities",
                Assertion = "Each entity has a stable identifier field and valid field types.",
                Automatable = true
            });
        }

        if (spec.ApiRoutes.Any())
        {
            rules.Add(new ValidationRuleDto
            {
                Id = "route-exists",
                Category = "route-exists",
                Description = "Declared API routes should be implemented.",
                Target = "apiRoutes",
                Assertion = "Every route in spec resolves to a handler implementation.",
                Automatable = true
            });
        }

        if (spec.ApiRoutes.Any(r => r.Auth))
        {
            rules.Add(new ValidationRuleDto
            {
                Id = "auth-guard",
                Category = "auth-guard",
                Description = "Protected API routes should enforce authentication.",
                Target = "apiRoutes",
                Assertion = "Routes marked with auth=true require authentication middleware.",
                Automatable = true
            });
        }

        return rules;
    }

    public static void EnsureHomePage(List<PageSpecDto> pages)
    {
        if (pages == null || pages.Count == 0)
            return;

        var existingRoutes = new HashSet<string>(
            pages.Select(page => NormalizePageRoute(page.Route)),
            StringComparer.OrdinalIgnoreCase);

        if (!existingRoutes.Add("/"))
            return;

        pages.Insert(0, BuildHomePageSpec(pages));
    }

    public static PageSpecDto BuildHomePageSpec(List<PageSpecDto> existingPages)
    {
        return new PageSpecDto
        {
            Route = "/",
            Name = "Home",
            Layout = InferHomePageLayout(existingPages),
            Components = new List<string>(),
            DataRequirements = new List<string>(),
            Description = "Recovered default home route for the application shell."
        };
    }

    public static string InferHomePageLayout(List<PageSpecDto> existingPages)
    {
        return existingPages.Any(page => string.Equals(page.Layout, "public", StringComparison.OrdinalIgnoreCase))
            ? "public"
            : "authenticated";
    }

    public static string InferTodoEntityName(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
            return string.Empty;

        if (source.Contains("todo", StringComparison.OrdinalIgnoreCase))
            return "Task";

        if (source.Contains("task", StringComparison.OrdinalIgnoreCase))
            return "Task";

        if (source.Contains("note", StringComparison.OrdinalIgnoreCase))
            return "Note";

        return string.Empty;
    }

    public static string ExtractRouteHint(ValidationRuleDto validation)
    {
        if (!string.IsNullOrWhiteSpace(validation.Target) && validation.Target.StartsWith('/'))
            return validation.Target;

        if (string.IsNullOrWhiteSpace(validation.Assertion))
            return string.Empty;

        var match = Regex.Match(validation.Assertion, @"/[A-Za-z0-9_\-/{}/:]+", RegexOptions.CultureInvariant);
        return match.Success ? match.Value : string.Empty;
    }

    public static string NormalizeFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return string.Empty;

        var normalized = filePath.Replace('\\', '/').Trim();

        if (normalized.StartsWith("./", StringComparison.Ordinal))
            normalized = normalized[2..];

        if (normalized.StartsWith("/", StringComparison.Ordinal))
            normalized = normalized[1..];

        if (normalized.Equals("project", StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        return normalized;
    }

    public static bool HasAnyFile(HashSet<string> filePaths, params string[] candidatePaths)
    {
        return candidatePaths
            .Select(NormalizeFilePath)
            .Any(filePaths.Contains);
    }

    public static bool HasStyledRoute(List<GeneratedFileDto> files, params string[] candidatePaths)
    {
        foreach (var candidatePath in candidatePaths)
        {
            var normalizedPath = NormalizeFilePath(candidatePath);
            var routeFile = files.FirstOrDefault(file =>
                NormalizeFilePath(file.Path).Equals(normalizedPath, StringComparison.OrdinalIgnoreCase));

            if (routeFile == null)
                continue;

            if (ContainsStyleHint(routeFile.Content))
                return true;
        }

        return false;
    }

    public static bool ContainsStyleHint(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return false;

        return content.Contains("className=", StringComparison.OrdinalIgnoreCase)
            || content.Contains("style={{", StringComparison.OrdinalIgnoreCase)
            || content.Contains("styles.", StringComparison.OrdinalIgnoreCase)
            || content.Contains("createStyles", StringComparison.OrdinalIgnoreCase)
            || content.Contains("styled(", StringComparison.OrdinalIgnoreCase)
            || content.Contains("styled.", StringComparison.OrdinalIgnoreCase)
            || Regex.IsMatch(content, @"import\s+.+\.(css|scss|sass|less)[""'];?", RegexOptions.IgnoreCase);
    }

    public static bool HasStyledRoute(List<GeneratedFile> files, params string[] targetPaths)
    {
        if (files == null || targetPaths == null || targetPaths.Length == 0)
            return false;

        var normalizedTargets = targetPaths.Select(NormalizeFilePath).ToList();

        foreach (var file in files)
        {
            var normalizedPath = NormalizeFilePath(file.Path);
            if (normalizedTargets.Contains(normalizedPath, StringComparer.OrdinalIgnoreCase))
            {
                var content = file.Content ?? string.Empty;
                // Look for common styling indicators (Tailwind classes, CSS-in-JS, UI libraries)
                return content.Contains("className=", StringComparison.OrdinalIgnoreCase) ||
                       content.Contains("style=", StringComparison.OrdinalIgnoreCase) ||
                       content.Contains("sx=", StringComparison.OrdinalIgnoreCase) ||
                       content.Contains("styled.", StringComparison.OrdinalIgnoreCase) ||
                       content.Contains("@emotion", StringComparison.OrdinalIgnoreCase);
            }
        }

        return false;
    }
}
