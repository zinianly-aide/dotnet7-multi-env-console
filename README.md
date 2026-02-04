# .NET 7 Multi-Environment Console Application

A .NET 7 console application with multi-module architecture, containerization, multi-environment support, logging, database connections, observability, and audit trails.

## Features

### Multi-Module Architecture
- **MultiModuleApp.Core** - Core business module
- **MultiModuleApp.Infrastructure** - Infrastructure (database)
- **MultiModuleApp.Logging** - Logging module
- **MultiModuleApp.Audit** - Audit module
- **MultiModuleApp.Observability** - Observability module
- **MultiModuleApp.Sync** - Data synchronization module

### Containerization
- **Docker** - Multi-environment Docker support
- **Docker Compose** - Development, Staging, Production configurations

### Logging System
- **Serilog** - Structured logging
- **Console + File Output** - Dual output channels
- **Log Levels** - Configurable severity levels

### Database Connections
- **Oracle** - Oracle database connection and data access
- **MySQL** - MySQL database connection and data access
- **Data Sync** - Bidirectional data synchronization between Oracle and MySQL

### Observability & Audit
- **Health Checks** - Application health monitoring
- **Metrics Collection** - Performance metrics gathering
- **Operation Audit Logs** - Complete audit trail
- **Request Tracking** - Request/response tracking

## Technologies

- **Framework**: .NET 7.0
- **ORM**: Entity Framework Core
- **Logging**: Serilog
- **Container**: Docker + Docker Compose
- **Version Control**: Git + GitHub
- **CI/CD**: GitHub Actions

## Multi-Environment Support

The application supports multiple environments:

### Environments
- **Development** (`ASPNETCORE_ENVIRONMENT=Development`)
  - Local development environment
  - Debug logging enabled
  - Detailed error messages

- **Staging** (`ASPNETCORE_ENVIRONMENT=Staging`)
  - Pre-production environment
  - Production-like logging
  - Staging database connections

- **Production** (`ASPNETCORE_ENVIRONMENT=Production`)
  - Production environment
  - Optimized logging
  - Production database connections

### Configuration Files
- `appsettings.json` - Base configuration (local development)
- `appsettings.Development.json` - Development environment
- `appsettings.Staging.json` - Staging environment
- `appsettings.Production.json` - Production environment

### Environment Switching

Using the `./scripts/switch-env.sh` script:

```bash
# Switch to development
./scripts/switch-env.sh development

# Switch to production
./scripts/switch-env.sh production

# Switch to staging
./scripts/switch-env.sh staging
```

## Getting Started

### Local Development

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build --configuration Release

# Run the application
dotnet run --configuration Release --project MultiModuleApp.Console
```

### Docker Deployment

#### Development Environment
```bash
docker-compose up -d
```

#### Production Environment
```bash
docker-compose -f docker-compose.prod.yml up -d
```

#### Staging Environment
```bash
docker-compose -f docker-compose.staging.yml up -d
```

## Project Structure

```
workspace/
├── src/
│   ├── MultiModuleApp.Console/          # Console application
│   ├── MultiModuleApp.Core/          # Core business module
│   ├── MultiModuleApp.Infrastructure/  # Infrastructure (database)
│   ├── MultiModuleApp.Logging/        # Logging module
│   ├── MultiModuleApp.Audit/          # Audit module
│   ├── MultiModuleApp.Observability/  # Observability module
│   └── MultiModuleApp.Sync/            # Data synchronization module
├── scripts/
│   ├── mysql/01-init.sql           # MySQL initialization
│   ├── oracle/01-init.sql          # Oracle initialization
│   ├── switch-env.sh              # Environment switching
│   └── validate-config.sh          # Configuration validation
├── .github/workflows/
│   └── dotnet.yml                   # GitHub Actions CI/CD workflow
├── Dockerfile                            # Docker container configuration
├── docker-compose.yml                      # Default (development)
├── docker-compose.prod.yml                 # Production
├── docker-compose.staging.yml              # Staging
└── .gitignore                          # Git ignore rules
```

## GitHub Actions CI/CD

### Workflow File
`.github/workflows/dotnet.yml`

### Triggers
- Push to `main` branch
- Pull request to `main` branch

### Steps
1. Checkout repository
2. Setup .NET 7.0.x SDK with NuGet caching
3. Restore NuGet packages
4. Build solution (Release configuration)
5. Run tests (if test projects exist)

### Workflow URL
https://github.com/zinianly-aide/dotnet7-multi-env-console/actions

## Version History

### v1.0.0 (2026-02-03)
- Initial project creation
- Multi-module architecture setup
- Containerization configuration
- Logging system implementation
- Database connection setup (Oracle + MySQL)
- Multi-environment support
- GitHub Actions CI/CD workflow
- Comprehensive documentation

### v1.0.1 (2026-02-03)
- Fixed OpenTelemetry API method names
- Fixed Configuration namespace reference
- Upgraded all projects to .NET 8.0
- Added OpenTelemetry Exporter packages
- Configured Metrics and Tracing

## Contributing

This is a .NET 7 console application with multi-module architecture and multi-environment support.

### Development Setup
1. Clone the repository
2. Install .NET 7.0 SDK
3. Restore NuGet packages: `dotnet restore`
4. Build the solution: `dotnet build --configuration Release`
5. Run the application: `dotnet run --configuration Release --project MultiModuleApp.Console`

### Environment Configuration
Copy `.env.example` to `.env` and configure:
- `ConnectionStrings__Oracle` - Oracle database connection string
- `ConnectionStrings__MySQL` - MySQL database connection string

### Running Tests
```bash
dotnet test --configuration Release
```

## License

MIT License

## Authors

- Created by OpenClaw Agent

## Acknowledgments

- .NET 7.0
- Entity Framework Core
- Serilog
- OpenTelemetry
- Oracle.ManagedDataAccess.Client
- MySQL Connector
- GitHub Actions
