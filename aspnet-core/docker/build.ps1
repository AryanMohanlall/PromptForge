# Build the .NET app locally and Docker image
$rootDir = Split-Path $PSScriptRoot -Parent
Push-Location $rootDir
dotnet publish src/ABPGroup.Web.Host/ABPGroup.Web.Host.csproj -c Release -o docker/publish
Pop-Location

# Build the Docker image so 'up' doesn't need --build
docker-compose build
