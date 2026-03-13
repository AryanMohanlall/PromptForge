FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src
COPY ["src/ABPGroup.Web.Host/ABPGroup.Web.Host.csproj", "src/ABPGroup.Web.Host/"]
COPY ["src/ABPGroup.Web.Core/ABPGroup.Web.Core.csproj", "src/ABPGroup.Web.Core/"]
COPY ["src/ABPGroup.Application/ABPGroup.Application.csproj", "src/ABPGroup.Application/"]
COPY ["src/ABPGroup.Core/ABPGroup.Core.csproj", "src/ABPGroup.Core/"]
COPY ["src/ABPGroup.EntityFrameworkCore/ABPGroup.EntityFrameworkCore.csproj", "src/ABPGroup.EntityFrameworkCore/"]
WORKDIR "/src/src/ABPGroup.Web.Host"
RUN dotnet restore 

WORKDIR /src
COPY ["src/ABPGroup.Web.Host", "src/ABPGroup.Web.Host"]
COPY ["src/ABPGroup.Web.Core", "src/ABPGroup.Web.Core"]
COPY ["src/ABPGroup.Application", "src/ABPGroup.Application"]
COPY ["src/ABPGroup.Core", "src/ABPGroup.Core"]
COPY ["src/ABPGroup.EntityFrameworkCore", "src/ABPGroup.EntityFrameworkCore"]
WORKDIR "/src/src/ABPGroup.Web.Host"
RUN dotnet publish -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0
EXPOSE 80
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "ABPGroup.Web.Host.dll"]
