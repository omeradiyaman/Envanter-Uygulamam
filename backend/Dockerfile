# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Root-level build configuration files (Directory.Build.props applies to all projects)
COPY Directory.Build.props NuGet.Config ./
COPY backend/InventorySystem.Domain/InventorySystem.Domain.csproj backend/InventorySystem.Domain/
COPY backend/InventorySystem.Application/InventorySystem.Application.csproj backend/InventorySystem.Application/
COPY backend/InventorySystem.Infrastructure/InventorySystem.Infrastructure.csproj backend/InventorySystem.Infrastructure/
COPY backend/InventorySystem.API/InventorySystem.API.csproj backend/InventorySystem.API/
RUN dotnet restore backend/InventorySystem.API/InventorySystem.API.csproj

COPY backend/ backend/

RUN dotnet publish backend/InventorySystem.API/InventorySystem.API.csproj \
    -c Release \
    --no-restore \
    -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "InventorySystem.API.dll"]
