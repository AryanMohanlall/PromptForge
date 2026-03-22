# ─────────────────────────────────────────────────────────────
# dev.ps1 — Run the backend locally (no Docker, fastest startup)
# ─────────────────────────────────────────────────────────────
# Usage:  .\dev.ps1
#
# Reads secrets from docker/.env so you don't duplicate config.
# Starts the API on http://localhost:44311
# ─────────────────────────────────────────────────────────────

$ErrorActionPreference = "Stop"
$envFile = Join-Path $PSScriptRoot "docker\.env"

# Load docker/.env as environment variables for this process
if (Test-Path $envFile) {
    Get-Content $envFile | ForEach-Object {
        $line = $_.Trim()
        if ($line -and -not $line.StartsWith("#")) {
            # Handle multi-line values (private keys etc.) — skip them,
            # only process simple KEY=VALUE lines
            if ($line -match '^([^=]+)=(.*)$') {
                $key = $Matches[1].Trim()
                $val = $Matches[2].Trim().Trim('"')
                [System.Environment]::SetEnvironmentVariable($key, $val, "Process")
            }
        }
    }
    Write-Host "[dev] Loaded environment from docker/.env" -ForegroundColor Green
} else {
    Write-Host "[dev] Warning: docker/.env not found — using appsettings.json defaults" -ForegroundColor Yellow
}

# Map docker-compose env var names to the format .NET config expects
$mappings = @{
    "ConnectionStrings__Default"  = "ConnectionStrings__Default"
    "Groq__ApiKey"                = "Groq__ApiKey"
    "Groq__Model"                 = "Groq__Model"
    "GitHub__ClientId"            = "GitHub__ClientId"
    "GitHub__ClientSecret"        = "GitHub__ClientSecret"
    "GitHub__RedirectUri"         = "GitHub__RedirectUri"
    "GitHubOAuth__ClientId"       = "GitHubOAuth__ClientId"
    "GitHubOAuth__ClientSecret"   = "GitHubOAuth__ClientSecret"
    "GitHubOAuth__RedirectUri"    = "GitHubOAuth__RedirectUri"
    "GitHubApp__AppId"            = "GitHubApp__AppId"
    "GitHubApp__InstallationId"   = "GitHubApp__InstallationId"
    "GitHubApp__PrivateKeyPem"    = "GitHubApp__PrivateKeyPem"
    "GitHubApp__WebhookSecret"    = "GitHubApp__WebhookSecret"
    "GitHubApp__Name"             = "GitHubApp__Name"
}

foreach ($entry in $mappings.GetEnumerator()) {
    $val = [System.Environment]::GetEnvironmentVariable($entry.Key, "Process")
    if ($val) {
        [System.Environment]::SetEnvironmentVariable($entry.Value, $val, "Process")
    }
}

# Local dev paths (not inside a container)
$localOutput = [System.Environment]::GetEnvironmentVariable("LOCAL_OUTPUT_PATH", "Process")
if (-not $localOutput) { 
    $localOutput = Join-Path ([System.IO.Path]::GetTempPath()) "PromptForge\GeneratedApps"
}
if (-not (Test-Path $localOutput)) { New-Item -Path $localOutput -ItemType Directory -Force | Out-Null }
[System.Environment]::SetEnvironmentVariable("CodeGen__LocalCopyPath", $localOutput, "Process")
[System.Environment]::SetEnvironmentVariable("CodeGen__OutputPath", $localOutput, "Process")

$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://localhost:44311"

Write-Host ""
Write-Host "╔══════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  ABPGroup Backend — Local Dev Mode           ║" -ForegroundColor Cyan
Write-Host "║  http://localhost:44311                      ║" -ForegroundColor Cyan
Write-Host "║  Swagger: http://localhost:44311/swagger     ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

Push-Location (Join-Path $PSScriptRoot "src\ABPGroup.Web.Host")
try {
    dotnet run
} finally {
    Pop-Location
}
