FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files (including test projects for restore)
COPY PointAtlas.sln ./
COPY PointAtlas.API/PointAtlas.API.csproj PointAtlas.API/
COPY PointAtlas.Application/PointAtlas.Application.csproj PointAtlas.Application/
COPY PointAtlas.Core/PointAtlas.Core.csproj PointAtlas.Core/
COPY PointAtlas.Infrastructure/PointAtlas.Infrastructure.csproj PointAtlas.Infrastructure/
COPY PointAtlas.API.Tests/PointAtlas.API.Tests.csproj PointAtlas.API.Tests/
COPY PointAtlas.Application.Tests/PointAtlas.Application.Tests.csproj PointAtlas.Application.Tests/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY . .

# Build and publish
WORKDIR /src/PointAtlas.API
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "PointAtlas.API.dll"]
