# Implementation Plan: Robust CodeGen Validations

Based on my analysis of your codebase, here's a comprehensive, phased implementation plan you can execute manually. I've mapped each phase to your existing architecture and identified the exact files you'll need to create or modify.

---

## Current State Assessment

Your existing validation system (`EvaluateValidationResults` in `CodeGenAppService.cs`) performs **string-based pattern matching**:
- Checks if `package.json` exists for `build-passes`
- Searches file content for route strings for `route-exists`
- Verifies shell files exist (`page.tsx`, `layout.tsx`)
- No actual compilation, AST analysis, or runtime testing

---

## Phase 1: Real-Time Compilation & Build Hooks

**Goal**: Replace the `package.json` existence check with actual `npm run build` execution.

### Step 1.1: Create the Build Validation Service

**New file**: `aspnet-core/src/ABPGroup.Application/CodeGen/Validation/BuildValidator.cs`

```
Responsibilities:
- Accept generated files as input
- Write files to a temp directory
- Run `npm install` and `npm run build`
- Capture stdout/stderr
- Return structured validation result with error line numbers
```

**Key methods to implement**:
- `ValidateBuildAsync(List<GeneratedFileDto> files, StackConfigDto stack)` → `BuildValidationResult`
- `WriteFilesToTempDir(List<GeneratedFileDto> files)` → `string tempDir`
- `RunProcessAsync(string workingDir, string command)` → `(int exitCode, string stdout, string stderr)`
- `CleanupTempDir(string tempDir)`

**Dependencies to add**:
- Use `System.Diagnostics.Process` for running shell commands
- Use `Path.GetTempPath()` + `Guid.NewGuid()` for isolated temp directories

### Step 1.2: Create the Build Validation Result DTO

**New file**: `aspnet-core/src/ABPGroup.Application/CodeGen/Dto/BuildValidationResultDto.cs`

```csharp
public class BuildValidationResultDto
{
    public bool Passed { get; set; }
    public int ExitCode { get; set; }
    public string Stdout { get; set; }
    public string Stderr { get; set; }
    public List<BuildErrorDto> Errors { get; set; } = new();
    public TimeSpan Duration { get; set; }
}

public class BuildErrorDto
{
    public string File { get; set; }
    public int? Line { get; set; }
    public int? Column { get; set; }
    public string Code { get; set; } // e.g., "TS2345"
    public string Message { get; set; }
    public string Severity { get; set; } // "error" | "warning"
}
```

### Step 1.3: Parse TypeScript/ESLint Output

**In `BuildValidator.cs`**, implement a regex parser for TSC output:
```
Pattern: src/app/page.tsx(12,5): error TS2322: Type 'string' is not assignable to type 'number'.
```

**Regex**: `^(?<file>.+?)\((?<line>\d+),(?<col>\d+)\):\s*(?<severity>error|warning)\s*(?<code>TS\d+):\s*(?<message>.+)$`

### Step 1.4: Integrate into `CodeGenAppService.EvaluateValidationResults`

**Modify**: `aspnet-core/src/ABPGroup.Application/CodeGen/CodeGenAppService.cs`

**Changes**:
1. Inject `BuildValidator` via constructor DI
2. In the `build-passes` case of `EvaluateValidationResults`, replace:
   ```csharp
   // OLD
   passed = filePaths.Contains("package.json");
   
   // NEW
   var buildResult = await _buildValidator.ValidateBuildAsync(files, stack);
   passed = buildResult.Passed;
   message = buildResult.Passed 
       ? "Build succeeded." 
       : $"Build failed with {buildResult.Errors.Count} errors.";
   ```

### Step 1.5: Feed Errors into Repair Loop

**Modify**: `RepairPrompts.BuildRepairPrompt`

**Changes**:
1. Accept `BuildValidationResultDto` as a new parameter
2. Add a "COMPILER ERRORS" section to the prompt:
   ```
   COMPILER ERRORS:
   - src/app/page.tsx(12,5): error TS2322: Type 'string' is not assignable to type 'number'.
   - src/components/Button.tsx(8,3): error TS2304: Cannot find name 'useState'.
   ```

### Step 1.6: Dependency Audit

**New file**: `aspnet-core/src/ABPGroup.Application/CodeGen/Validation/DependencyValidator.cs`

```
Responsibilities:
- Parse all import statements from generated files
- Extract package names (handle scoped packages like @ant-design/icons)
- Compare against dependencyPlan.dependencies and dependencyPlan.devDependencies
- Flag missing or unused dependencies
```

**Add to `ValidationRuleDto.Category` options**: `"dependency-audit"`

---

## Phase 2: AST-Based Structural Analysis

**Goal**: Move beyond string matching to understand code structure.

### Step 2.1: Create the AST Analyzer Service

**New file**: `aspnet-core/src/ABPGroup.Application/CodeGen/Validation/AstAnalyzer.cs`

**Approach**: Since your backend is .NET, you have two options:
- **Option A**: Call a Node.js script via `Process` that uses the TypeScript Compiler API
- **Option B**: Use a .NET TypeScript parser (limited ecosystem)

**Recommended**: Option A — create a small Node.js helper script.

**New file**: `aspnet-core/src/ABPGroup.Application/CodeGen/Validation/ast-analyzer.js`

```javascript
// Uses TypeScript Compiler API to parse and analyze code
// Receives file paths via stdin, returns AST analysis via stdout
// Can detect: imports, exports, component props, function signatures
```

### Step 2.2: Entity-DbContext Relationship Verification

**Modify**: `CodeGenAppService.EvaluateValidationResults`

**New validation category**: `"entity-dbcontext-sync"`

**Logic**:
1. Parse all `EntitySpec` definitions from the spec
2. For each entity, check if a corresponding model/interface exists in generated files
3. Verify field names match between spec and generated code

**Implementation**:
```csharp
case "entity-dbcontext-sync":
{
    var entities = spec.Entities ?? new List<EntitySpecDto>();
    var allContent = string.Join("\n", files.Select(f => f.Content));
    
    foreach (var entity in entities)
    {
        var hasEntityDefinition = allContent.Contains($"interface {entity.Name}") 
            || allContent.Contains($"class {entity.Name}")
            || allContent.Contains($"type {entity.Name}");
        
        if (!hasEntityDefinition)
        {
            passed = false;
            message = $"Entity '{entity.Name}' has no corresponding type definition in generated code.";
            break;
        }
    }
    break;
}
```

### Step 2.3: Page Route Verification

**New validation category**: `"page-route-mapping"`

**Logic**:
1. For each `PageSpec.Route`, verify the corresponding file exists in the file manifest
2. For Next.js: `/tasks` → `src/app/tasks/page.tsx`
3. For Vite: `/tasks` → route defined in `src/App.tsx`

### Step 2.4: Type Safety Checks

**New validation category**: `"type-safety"`

**Logic**:
1. Use the AST analyzer to extract component prop types
2. Verify that props passed match the `DataRequirements` in `PageSpec`
3. Flag mismatches between expected and actual prop types

---

## Phase 3: Automated Contract Testing

**Goal**: Verify application logic actually works.

### Step 3.1: Create Test Generator

**New file**: `aspnet-core/src/ABPGroup.Application/CodeGen/Validation/TestGenerator.cs`

**Responsibilities**:
- Generate Vitest unit tests for API routes
- Generate Playwright E2E tests for page routes
- Tests are derived from `ApiRouteSpec` and `PageSpec`

**Example generated test**:
```typescript
// __tests__/api/tasks.test.ts
import { describe, it, expect } from 'vitest';

describe('GET /api/tasks', () => {
  it('should return an array of tasks', async () => {
    const response = await fetch('http://localhost:3000/api/tasks');
    expect(response.status).toBe(200);
    const data = await response.json();
    expect(Array.isArray(data)).toBe(true);
  });
});
```

### Step 3.2: Create Test Runner

**New file**: `aspnet-core/src/ABPGroup.Application/CodeGen/Validation/TestRunner.cs`

**Responsibilities**:
- Write generated tests to temp directory alongside generated code
- Run `npx vitest run` or `npx playwright test`
- Parse test results
- Return structured failures

### Step 3.3: Add Validation Category

**New category**: `"test-passes"` (already exists in your enum but not fully implemented)

**Modify `EvaluateValidationResults`**:
```csharp
case "test-passes":
{
    if (!validation.Automatable)
    {
        passed = true;
        message = "Test validation skipped (not automatable).";
        break;
    }
    
    var testResult = await _testRunner.RunTestsAsync(files, stack);
    passed = testResult.Passed;
    message = testResult.Passed 
        ? $"All {testResult.TotalTests} tests passed."
        : $"{testResult.FailedTests}/{testResult.TotalTests} tests failed.";
    break;
}
```

### Step 3.4: Data Integrity Verification

**New validation category**: `"data-integrity"`

**Logic**:
1. For each `ApiRouteSpec` with method GET, generate a test that:
   - Makes a request to the endpoint
   - Verifies response shape matches `ResponseShape`
   - Verifies all fields from `EntitySpec` are present

---

## Phase 4: AI Reviewer Agent (The "Adversary")

**Goal**: Integrate a second LLM that reviews code against the spec.

### Step 4.1: Create the AI Reviewer Service

**New file**: `aspnet-core/src/ABPGroup.Application/CodeGen/Validation/AiReviewer.cs`

**Responsibilities**:
- Take the `AppSpecDto` and generated files as input
- Call a separate LLM (can use same Gemini API with different prompt)
- Return structured review results

### Step 4.2: Create the Review Prompt Template

**New file**: `aspnet-core/src/ABPGroup.Application/CodeGen/PromptTemplates/ReviewerPrompts.cs`

```csharp
public static string BuildReviewPrompt(AppSpecDto spec, List<GeneratedFileDto> files)
{
    return $@"You are a strict code reviewer. Your job is to verify that generated code 
matches the specification EXACTLY.

SPECIFICATION:
{JsonSerializer.Serialize(spec)}

GENERATED FILES:
{string.Join("\n\n", files.Select(f => $"--- {f.Path} ---\n{f.Content}"))}

For each validation rule, determine if the code COMPLIES. You MUST cite specific 
line numbers as evidence.

Return your review as JSON:
{{
  ""reviews"": [
    {{
      ""ruleId"": ""entity-schema"",
      ""passed"": true/false,
      ""evidence"": ""Line 12-15 of src/types/Task.ts defines the interface"",
      ""issues"": [""Missing 'completed' field in Task interface""]
    }}
  ]
}}";
}
```

### Step 4.3: Evidence-Based Success Criteria

**Modify**: `ValidationResultDto` to include evidence:

**Add to DTO**:
```csharp
public class ValidationResultDto
{
    public string Id { get; set; }
    public string Status { get; set; }
    public string Message { get; set; }
    // NEW
    public string Evidence { get; set; } // Line numbers proving compliance
    public List<string> Issues { get; set; } = new(); // Specific issues found
}
```

### Step 4.4: Integrate into Validation Pipeline

**Modify**: `CodeGenAppService.EvaluateValidationResults`

After all other validations run, call the AI reviewer for a second opinion:
```csharp
// After existing validations
var aiReview = await _aiReviewer.ReviewAsync(spec, files, stack);
foreach (var review in aiReview.Reviews)
{
    var existing = results.FirstOrDefault(r => r.Id == review.RuleId);
    if (existing != null && existing.Status == "passed" && !review.Passed)
    {
        // AI found an issue that pattern matching missed
        existing.Status = "failed";
        existing.Message = $"AI Review: {string.Join("; ", review.Issues)}";
        existing.Evidence = review.Evidence;
    }
}
```

---

## Supporting Infrastructure

### New Validation Categories to Add

Update `IValidationRule.category` in `context.tsx` and `NormalizeValidationCategory` in `CodeGenAppService.cs`:

```csharp
"dependency-audit"    // Phase 1
"entity-dbcontext-sync"  // Phase 2
"page-route-mapping"  // Phase 2
"type-safety"         // Phase 2
"data-integrity"      // Phase 3
"ai-review"           // Phase 4
```

### Configuration

**Add to `appsettings.json`**:
```json
{
  "CodeGen": {
    "Validation": {
      "EnableBuildValidation": true,
      "BuildTimeoutSeconds": 120,
      "EnableTestGeneration": true,
      "EnableAiReview": true,
      "TempDirBasePath": "/tmp/codegen-validation"
    }
  }
}
```

---

## Implementation Order (Recommended)

| Week | Phase | Deliverable |
|------|-------|-------------|
| 1 | 1.1–1.4 | BuildValidator working end-to-end |
| 2 | 1.5–1.6 | Repair loop uses compiler errors + dependency audit |
| 3 | 2.1–2.2 | AST analyzer + entity sync validation |
| 4 | 2.3–2.4 | Route mapping + type safety checks |
| 5 | 3.1–3.2 | Test generation + execution |
| 6 | 3.3–3.4 | Test validation integrated into pipeline |
| 7 | 4.1–4.2 | AI reviewer service + prompt |
| 8 | 4.3–4.4 | Evidence-based validation + full integration |

---

## Quick Wins (Start Here)

1. **Immediate**: Add `BuildValidator.cs` with basic `npm run build` execution
2. **Day 1**: Parse TSC errors and feed them into `RepairPrompts`
3. **Day 2**: Add dependency audit validation
4. **Day 3**: Add entity-DbContext sync validation (string-based, no AST yet)

These quick wins will dramatically improve validation quality before you tackle the more complex AST and testing phases.