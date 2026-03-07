# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/Catalog/Beyond8.Catalog.Api/Beyond8.Catalog.Api.csproj", "src/Services/Catalog/Beyond8.Catalog.Api/"]
COPY ["shared/Beyond8.DatabaseMigrationHelpers/Beyond8.DatabaseMigrationHelpers.csproj", "shared/Beyond8.DatabaseMigrationHelpers/"]
COPY ["src/Orchestration/Beyond8.ServiceDefaults/Beyond8.ServiceDefaults.csproj", "src/Orchestration/Beyond8.ServiceDefaults/"]
COPY ["src/Services/Catalog/Beyond8.Catalog.Application/Beyond8.Catalog.Application.csproj", "src/Services/Catalog/Beyond8.Catalog.Application/"]
COPY ["src/Services/Catalog/Beyond8.Catalog.Domain/Beyond8.Catalog.Domain.csproj", "src/Services/Catalog/Beyond8.Catalog.Domain/"]
COPY ["shared/Beyond8.Common/Beyond8.Common.csproj", "shared/Beyond8.Common/"]
COPY ["src/Services/Catalog/Beyond8.Catalog.Infrastructure/Beyond8.Catalog.Infrastructure.csproj", "src/Services/Catalog/Beyond8.Catalog.Infrastructure/"]
RUN dotnet restore "./src/Services/Catalog/Beyond8.Catalog.Api/Beyond8.Catalog.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/Catalog/Beyond8.Catalog.Api"
RUN dotnet build "./Beyond8.Catalog.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Beyond8.Catalog.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Beyond8.Catalog.Api.dll"]
