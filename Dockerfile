# Build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["MultiModuleApp.sln", "./"]
COPY ["src/MultiModuleApp.Core/MultiModuleApp.Core.csproj", "src/MultiModuleApp.Core/"]
COPY ["src/MultiModuleApp.Infrastructure/MultiModuleApp.Infrastructure.csproj", "src/MultiModuleApp.Infrastructure/"]
COPY ["src/MultiModuleApp.Logging/MultiModuleApp.Logging.csproj", "src/MultiModuleApp.Logging/"]
COPY ["src/MultiModuleApp.Observability/MultiModuleApp.Observability.csproj", "src/MultiModuleApp.Observability/"]
COPY ["src/MultiModuleApp.Audit/MultiModuleApp.Audit.csproj", "src/MultiModuleApp.Audit/"]
COPY ["src/MultiModuleApp.Sync/MultiModuleApp.Sync.csproj", "src/MultiModuleApp.Sync/"]
COPY ["MultiModuleApp.Console/MultiModuleApp.Console.csproj", "MultiModuleApp.Console/"]

# Restore dependencies
RUN dotnet restore "MultiModuleApp.sln"

# Copy source code
COPY . .

# Build the project
WORKDIR "/src/MultiModuleApp.Console"
RUN dotnet build "MultiModuleApp.Console.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "MultiModuleApp.Console.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app

# Install dependencies
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libc6-dev \
    && rm -rf /var/lib/apt/lists/*

# Create logs directory
RUN mkdir -p /app/logs

# Copy published app
COPY --from=publish /app/publish .

# Set environment variable
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port (if needed for health checks)
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "MultiModuleApp.Console.dll"]
