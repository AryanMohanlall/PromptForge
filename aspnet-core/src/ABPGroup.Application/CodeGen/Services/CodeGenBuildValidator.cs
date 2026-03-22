using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;
using ABPGroup.Projects;
using Microsoft.Extensions.Logging;

namespace ABPGroup.CodeGen;

public class CodeGenBuildValidator : ICodeGenBuildValidator, ITransientDependency
{
    private readonly ILogger<CodeGenBuildValidator> _logger;

    public CodeGenBuildValidator(ILogger<CodeGenBuildValidator> logger)
    {
        _logger = logger;
    }

    public async Task<BuildValidationResult> ValidateBuildAsync(string outputPath, Framework framework)
    {
        var result = new BuildValidationResult();
        if (string.IsNullOrEmpty(outputPath) || !Directory.Exists(outputPath))
        {
            result.Success = false;
            result.Errors.Add($"Output path {outputPath} does not exist.");
            return result;
        }

        try
        {
            // Normalize path for build execution
            var normalizedPath = Path.GetFullPath(outputPath);

            if (framework == Framework.NextJS)
            {
                return await ValidateNextJsBuildAsync(normalizedPath);
            }
            if (framework == Framework.DotNetBlazor)
            {
                return await ValidateDotNetBuildAsync(normalizedPath);
            }

            result.Success = true;
            result.Logs = "No build validator implemented for this framework yet. Skipping validation...";
            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add(ex.Message);
            return result;
        }
    }

    private async Task<BuildValidationResult> ValidateNextJsBuildAsync(string outputPath)
    {
        var result = new BuildValidationResult();
        var logs = new StringBuilder();

        _logger.LogInformation($"Validating Next.js build in {outputPath}");

        // 1. npm install (only if node_modules missing and package.json exists)
        if (!File.Exists(Path.Combine(outputPath, "package.json")))
        {
            result.Success = false;
            result.Errors.Add("package.json not found in output directory.");
            return result;
        }

        if (!Directory.Exists(Path.Combine(outputPath, "node_modules")))
        {
            _logger.LogInformation("Running npm install...");
            var installResult = await RunCommandAsync("npm", "install --no-audit --no-fund --prefer-offline", outputPath, logs);
            if (!installResult)
            {
                result.Success = false;
                result.Logs = logs.ToString();
                result.Errors.Add("npm install failed. See logs for details.");
                return result;
            }
        }

        // 2. npm run build
        _logger.LogInformation("Running npm run build...");
        var buildResult = await RunCommandAsync("npm", "run build", outputPath, logs);
        result.Success = buildResult;
        result.Logs = logs.ToString();
        if (!buildResult)
        {
            result.Errors.Add("npm run build failed. See logs for details.");
        }

        return result;
    }

    private async Task<BuildValidationResult> ValidateDotNetBuildAsync(string outputPath)
    {
        var result = new BuildValidationResult();
        var logs = new StringBuilder();

        _logger.LogInformation($"Validating .NET build in {outputPath}");

        var buildResult = await RunCommandAsync("dotnet", "build", outputPath, logs);
        result.Success = buildResult;
        result.Logs = logs.ToString();
        if (!buildResult)
        {
            result.Errors.Add("dotnet build failed. See logs for details.");
        }

        return result;
    }

    private async Task<bool> RunCommandAsync(string cmd, string args, string workingDir, StringBuilder logs)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = cmd,
            Arguments = args,
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            // Ensure we don't block on stdin
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        if (OperatingSystem.IsWindows())
        {
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = $"/c {cmd} {args}";
        }

        logs.AppendLine($"[COMMAND] {cmd} {args}");
        
        using var process = new Process { StartInfo = startInfo };
        
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (s, e) => { if (e.Data != null) outputBuilder.AppendLine(e.Data); };
        process.ErrorDataReceived += (s, e) => { if (e.Data != null) errorBuilder.AppendLine(e.Data); };

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Timeout after 15 minutes (npm install can be slow)
            var timeoutTask = Task.Delay(TimeSpan.FromMinutes(15));
            var processTask = Task.Run(() => process.WaitForExit());

            if (await Task.WhenAny(timeoutTask, processTask) == timeoutTask)
            {
                try { process.Kill(true); } catch { }
                logs.AppendLine("Process timed out after 15 minutes.");
                return false;
            }

            logs.Append(outputBuilder.ToString());
            if (errorBuilder.Length > 0)
            {
                logs.AppendLine("--- ERROR OUTPUT ---");
                logs.Append(errorBuilder.ToString());
            }

            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            logs.AppendLine($"Failed to start process: {ex.Message}");
            return false;
        }
    }
}
