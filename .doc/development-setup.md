[Back to README](../README.md)

## Development Setup

This document describes how to configure, run and test the API in a development environment.

### Prerequisites

- Docker Desktop with Docker Compose enabled.
- .NET SDK 8.0 when running the application or tests outside containers.
- The ports `8080`, `5432`, `27017`, `6379`, `5433` and `27018` available for the selected workflow.

Run all commands from the repository root.

### Start the application with Docker Compose

The development Compose stack starts the API, PostgreSQL, MongoDB and Redis:

```powershell
docker compose up -d --build
```

Check the service status:

```powershell
docker compose ps
```

View API logs:

```powershell
docker compose logs -f ambev.developerevaluation.webapi
```

Stop the services:

```powershell
docker compose down
```

Development environment ports:

| Service | Local port | Purpose |
|---|---:|---|
| API | `8080` | HTTP, Swagger and functional tests |
| PostgreSQL | `5432` | Main database |
| MongoDB | `27017` | Domain events |
| Redis | `6379` | Available Compose service |

### API startup

During startup, the API automatically:

1. Applies Entity Framework migrations to PostgreSQL.
2. Creates the default administrator, only in the `Development` environment.
3. Configures JWT authentication, authorization, health checks and Swagger.

Default administrator credentials:

```text
Email:    admin@localhost
Password: Admin@123
```

Swagger is available at:

```text
http://localhost:8080/swagger
```

Health checks:

```text
http://localhost:8080/health
http://localhost:8080/health/live
http://localhost:8080/health/ready
```

### Configuration

Base configuration is stored in `src/Ambev.DeveloperEvaluation.WebApi/appsettings.json`. Development-specific settings are stored in `appsettings.Development.json`.

Docker Compose overrides the database connections to use container names:

```text
ConnectionStrings__DefaultConnection=Host=ambev.developerevaluation.database;Database=developer_evaluation;Username=developer;Password=ev@luAt10n
ConnectionStrings__MongoDb=mongodb://developer:ev%40luAt10n@ambev.developerevaluation.nosql:27017/?authSource=admin
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_HTTP_PORTS=8080
```

To run the API outside Docker, `appsettings.json` uses `localhost` for PostgreSQL and MongoDB. The local HTTP profile uses port `5119`:

```powershell
dotnet run --project .\src\Ambev.DeveloperEvaluation.WebApi\Ambev.DeveloperEvaluation.WebApi.csproj --launch-profile http
```

In this workflow, start the databases first:

```powershell
docker compose up -d ambev.developerevaluation.database ambev.developerevaluation.nosql
```

The local profile is available at `http://localhost:5119/swagger`.

Useful environment variables:

| Variable | Description |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | Application environment; use `Development` to enable seed data and Swagger |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `ConnectionStrings__MongoDb` | MongoDB connection string |
| `MongoDb__Database` | MongoDB database name |
| `Jwt__SecretKey` | Secret used to sign JWT tokens |
| `Seed__Admin__Email` | Initial administrator email |
| `Seed__Admin__Password` | Initial administrator password |
| `FUNCTIONAL_TEST_BASE_URL` | URL used by functional tests; defaults to `http://localhost:8080` |

Example of a temporary PowerShell override:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:Jwt__SecretKey = "DevelopmentSecretKeyWithAtLeast32Characters"
```

The credentials and key above are for development only. Use secrets or protected environment variables in other environments.

### Migrations

Manual migration execution is normally not required because the API calls `Database.MigrateAsync()` during startup.

To apply migrations manually with the Entity Framework CLI:

```powershell
dotnet ef database update `
  --project .\src\Ambev.DeveloperEvaluation.ORM\Ambev.DeveloperEvaluation.ORM.csproj `
  --startup-project .\src\Ambev.DeveloperEvaluation.WebApi\Ambev.DeveloperEvaluation.WebApi.csproj
```

### Tests

Run the complete solution:

```powershell
dotnet test .\Ambev.DeveloperEvaluation.sln --no-restore
```

Run a single test category:

```powershell
dotnet test .\tests\Ambev.DeveloperEvaluation.Unit\Ambev.DeveloperEvaluation.Unit.csproj --no-restore
dotnet test .\tests\Ambev.DeveloperEvaluation.Integration\Ambev.DeveloperEvaluation.Integration.csproj --no-restore
dotnet test .\tests\Ambev.DeveloperEvaluation.Functional\Ambev.DeveloperEvaluation.Functional.csproj --no-restore
```

Integration tests use isolated databases on ports `5433` and `27018`:

```powershell
docker compose -f .\docker-compose.test.yml up -d
dotnet test .\tests\Ambev.DeveloperEvaluation.Integration\Ambev.DeveloperEvaluation.Integration.csproj --no-restore
```

Functional tests require the API to be available at `http://localhost:8080`. To use another URL:

```powershell
$env:FUNCTIONAL_TEST_BASE_URL = "http://localhost:5119"
dotnet test .\tests\Ambev.DeveloperEvaluation.Functional\Ambev.DeveloperEvaluation.Functional.csproj --no-restore
```

### Common issues

- **Port already in use:** change the Compose port mapping or stop the process using the port.
- **API starts before PostgreSQL:** wait for the containers and run `docker compose restart ambev.developerevaluation.webapi`.
- **Administrator login fails:** confirm that the environment is `Development` and that the database contains the expected seed data.
- **Swagger is unavailable:** confirm that `ASPNETCORE_ENVIRONMENT=Development` is set.
- **Functional tests cannot connect:** start the API and check `FUNCTIONAL_TEST_BASE_URL`.
