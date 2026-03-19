FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
RUN apt-get update && apt-get install -y --no-install-recommends curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y --no-install-recommends nodejs && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

FROM base AS final
EXPOSE 80
WORKDIR /app
COPY docker/publish/ .
ENTRYPOINT ["dotnet", "ABPGroup.Web.Host.dll"]
