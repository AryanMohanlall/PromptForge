using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Abp.Domain.Services;
using ABPGroup.Projects;

namespace ABPGroup.CodeGen;

public class CodeGenScaffolder : DomainService, ICodeGenScaffolder
{
    public string FindTemplateDirectory(string templateSlug, string currentDir = null)
    {
        // Walk up from the current directory and base directory to find the frontend/templates folder
        var roots = new[] { currentDir ?? Directory.GetCurrentDirectory(), AppDomain.CurrentDomain.BaseDirectory };
        foreach (var root in roots)
        {
            var dir = root;
            for (var i = 0; i < 8; i++)
            {
                var candidate = Path.GetFullPath(Path.Combine(dir, "frontend", "templates", templateSlug));
                if (Directory.Exists(candidate)) return candidate;
                var parent = Directory.GetParent(dir)?.FullName;
                if (parent == null || parent == dir) break;
                dir = parent;
            }
        }

        return null;
    }

    public List<GeneratedFile> ReadScaffoldFiles(string templateDir)
    {
        var files = new List<GeneratedFile>();
        var allFiles = Directory.GetFiles(templateDir, "*", SearchOption.AllDirectories);

        foreach (var filePath in allFiles)
        {
            var relativePath = Path.GetRelativePath(templateDir, filePath).Replace('\\', '/');
            // Skip hidden directories and node_modules
            if (relativePath.StartsWith(".git/") || relativePath.Contains("node_modules/"))
                continue;

            try
            {
                var content = File.ReadAllText(filePath);
                files.Add(new GeneratedFile { Path = relativePath, Content = content });
            }
            catch
            {
                // Skip binary/unreadable files
            }
        }

        return files;
    }

    public void WriteFilesToDisk(List<GeneratedFile> files, string outputPath)
    {
        foreach (var file in files)
        {
            var fullPath = Path.Combine(outputPath, file.Path.Replace('/', Path.DirectorySeparatorChar));
            var dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(fullPath, file.Content);
        }
    }

    public void AddScaffoldFiles(List<GeneratedFile> files, Framework framework, string currentDir)
    {
        if (framework != Framework.NextJS)
            return;

        var templateDir = FindTemplateDirectory("next-ts-antd-prisma", currentDir);
        if (templateDir == null)
            return;

        files.AddRange(ReadScaffoldFiles(templateDir));
    }

    public void AddApprovedReadmeFile(List<GeneratedFile> files, string approvedReadme)
    {
        if (string.IsNullOrWhiteSpace(approvedReadme))
            return;

        var existingReadme = files.FirstOrDefault(file =>
            string.Equals(CodeGenHelpers.NormalizeFilePath(file.Path), "readme.md", StringComparison.OrdinalIgnoreCase));

        if (existingReadme != null)
        {
            existingReadme.Content = approvedReadme;
            return;
        }

        files.Add(new GeneratedFile
        {
            Path = "README.md",
            Content = approvedReadme
        });
    }
}
