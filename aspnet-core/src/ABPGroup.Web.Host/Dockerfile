# FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
# RUN apt-get update && apt-get install -y --no-install-recommends curl && \
#     curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
#     apt-get install -y --no-install-recommends nodejs && \
#     apt-get clean && rm -rf /var/lib/apt/lists/*

# FROM base AS final
# EXPOSE 80
# WORKDIR /app
# COPY docker/publish/ .
# ENTRYPOINT ["dotnet", "ABPGroup.Web.Host.dll"]

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .
RUN find . -name "*.csproj" | head -20
RUN dotnet restore "src/ABPGroup.Web.Host/ABPGroup.Web.Host.csproj"
RUN dotnet publish "src/ABPGroup.Web.Host/ABPGroup.Web.Host.csproj" \
    -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
RUN apt-get update && apt-get install -y --no-install-recommends curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y --no-install-recommends nodejs && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

FROM base AS final
EXPOSE 80
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ABPGroup.Web.Host.dll"]
