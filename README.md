# EnterpriseOperationsAPI — Production-Oriented ASP.NET Core Web API (.NET 10)

EnterpriseOperationsAPI is a backend-focused ASP.NET Core Web API that applies production-oriented patterns used in modern .NET services.

The solution separates domain, application, infrastructure, and API concerns and extends beyond CRUD with authentication and role-based authorization, configurable caching, resilient external HTTP integration, optimistic concurrency, background processing, observability, integration testing against SQL Server, containerization, and automated deployment to Azure.

## Architecture

The solution follows Clean Architecture principles with dependencies directed toward the core.

```text
Domain
└── no project dependencies

Application
└── Domain

Infrastructure
├── Application
└── Domain

API
├── Application
├── Infrastructure
└── Domain
```

Responsibilities are divided as follows:

- **Domain** — core entities and domain state.
- **Application** — DTOs, service and repository contracts, query models, application services, settings, and infrastructure abstractions.
- **Infrastructure** — EF Core persistence, repository implementations, ASP.NET Core Identity stores, caching implementations, external HTTP integration, database initialization, and background jobs.
- **API** — controllers, middleware, health endpoints, OpenTelemetry registration, application startup, and HTTP pipeline configuration.

The Application layer defines abstractions such as repositories and caching services; Infrastructure supplies their concrete implementations, while the API project acts as the composition and runtime boundary.

## Tech Stack

**Core:** C# · .NET 10 · ASP.NET Core Web API · REST

**Architecture & Data:** Clean Architecture · Entity Framework Core · SQL Server · Repository Pattern · Service Layer · DTOs · EF Core Migrations · SQL Indexes

**Security:** ASP.NET Core Identity · Bearer Authentication · Authorization · Role-Based Access Control

**Caching & Resilience:** IMemoryCache · Redis · Distributed Caching · HttpClient · Microsoft.Extensions.Http.Resilience · Retry · Timeout · Fallback

**Observability & Processing:** Structured Logging · OpenTelemetry · Health Checks · Hangfire

**Testing:** xUnit · Moq · WebApplicationFactory · Testcontainers · SQL Server Integration Tests

**Containers, Cloud & DevOps:** Docker · Docker Compose · GitHub Container Registry (GHCR) · GitHub Actions · CI/CD · OpenID Connect (OIDC) · Microsoft Entra ID · Azure RBAC · Azure Container Apps · Azure SQL Database

## API and Data Access

`OperationTask` is the primary domain resource. Authenticated API operations support CRUD together with server-side pagination, completion-state filtering, text search, configurable sorting, and ascending or descending sort direction.

Read queries use EF Core `AsNoTracking` where change tracking is unnecessary. Paged queries execute filtering and sorting before `CountAsync`, `Skip`, and `Take`, returning both records and pagination metadata. Database indexes support frequently queried fields including completion state and creation time.

DTOs form the API/application boundary rather than exposing persistence entities directly.

## Authentication and Authorization

Authentication is implemented with ASP.NET Core Identity backed by EF Core and SQL Server.

Identity API endpoints provide registration and token-based login. Application endpoints can require authenticated requests, and Identity roles support role-based authorization, including administrative access to the Hangfire dashboard when background processing is enabled.

Development users and roles are created only when development seeding is explicitly enabled. Production deployment does not seed development accounts.

## Caching and Invalidation

The Application layer defines `ICacheService`, with in-memory and Redis-backed implementations selected through configuration.

Paged query results are cached with keys derived from the query state and a shared cache version:

```text
operation-tasks:paged:v{version}:...
```

Create, update, and delete operations increment the shared version. Subsequent reads therefore use a new cache-key namespace instead of requiring enumeration and deletion of every previously cached paged query.

The Redis implementation performs the version change with Redis `INCR` through `StringIncrementAsync`, providing an atomic version increment. Cached values use configurable absolute expiration.

This allows local/containerized environments to use Redis while the current Azure deployment uses in-memory caching without changing application-service code.

## External HTTP Resilience

External HTTP communication is isolated behind `IExternalSystemService` and implemented through a configured `HttpClient`.

The HTTP pipeline uses `AddStandardResilienceHandler` with configuration-driven:

- total request timeout
- per-attempt timeout
- retry count
- retry delay

When the external request succeeds, the service caches the response as a last-known-good value. If the dependency remains unavailable after the configured resilience behavior, the service returns that cached value when available; otherwise it returns an explicit unavailable fallback response.

The retry/timeout pipeline and application-level fallback remain separate concerns.

## Error Handling and Optimistic Concurrency

Unhandled exceptions are processed centrally through ASP.NET Core `IExceptionHandler`. Errors are logged and returned as `ProblemDetails` responses with a request trace identifier, while internal exception details are not exposed to clients.

`OperationTask` updates use SQL Server `rowversion` optimistic concurrency. The row version returned to the client is represented as Base64 in the DTO contract and supplied on update. The repository assigns it as EF Core's original concurrency value before saving.

If another request has modified the row first, EF Core raises `DbUpdateConcurrencyException`, which the global handler maps to HTTP `409 Conflict`. This prevents silent lost updates without pessimistic database locking.

## Observability and Health Checks

The API uses structured `ILogger` logging and OpenTelemetry tracing.

Tracing includes:

- ASP.NET Core request instrumentation
- outbound `HttpClient` instrumentation
- a custom `EnterpriseOperations.Application` `ActivitySource`
- application tags for pagination, filtering, sorting, and cache hit/miss behavior
- console export for the configured telemetry pipeline

Health endpoints separate process liveness from dependency readiness:

```text
/health/live
/health/ready
```

`/health/live` verifies that the application is responding without evaluating dependency checks. `/health/ready` runs checks tagged as ready, including an EF Core `AppDbContext` check for SQL Server connectivity.

## Background Processing

Hangfire provides recurring background processing with SQL Server storage.

When `BackgroundJobs:Enabled` is true, the application registers the Hangfire server and schedules the external-system status check as a recurring hourly job. The dashboard is enabled conditionally and protected by a custom authorization filter.

Keeping the feature configuration-controlled allows environments that do not require background processing to run without the Hangfire server or dashboard.

## Testing Strategy

The solution separates unit and integration tests.

### Unit Tests

xUnit and Moq test application-service behavior with repository, cache, and configuration dependencies isolated.

### Integration Tests

Integration tests execute the real ASP.NET Core request pipeline through `WebApplicationFactory<Program>` and a custom application factory.

A SQL Server 2022 container is provisioned with Testcontainers for the integration suite. The Testing environment injects the container connection string, applies EF Core migrations on startup, disables development-data seeding and background jobs, and selects in-memory caching.

This tests EF Core against SQL Server rather than substituting an in-memory persistence provider, allowing database-specific behavior such as migrations and `rowversion` concurrency to be exercised.

The integration suite covers:

- authentication and bearer-token login
- authorization and role-based access
- liveness and readiness endpoints
- authenticated CRUD operations
- not-found behavior
- pagination, filtering, and sorting
- optimistic concurrency conflicts

The completed solution currently contains **22 automated tests**, all executed by CI.

## Containerized Local Development

Docker Compose provides a local multi-container environment:

```text
ASP.NET Core API
SQL Server
Redis
```

The API uses a multi-stage Docker build. Local credentials are supplied through environment configuration: `.env.example` documents the required variable shape while the real `.env` file is excluded from source control.

### Prerequisites

- .NET 10 SDK
- Docker Desktop

### Run with Docker Compose

```bash
git clone https://github.com/AlanRacic/EnterpriseOperationsAPI.git
cd EnterpriseOperationsAPI
```

Create `.env` from `.env.example` and replace the example SQL Server password with a local development password.

Then start the environment:

```bash
docker compose up --build
```

The API container listens on port `8080`.

## CI/CD Pipeline

Three GitHub Actions workflows separate validation, image publishing, and deployment:

```text
Push / Pull Request to master
          │
          ▼
         CI
Restore → Release Build → Unit + Integration Tests
          │
          │ successful master run
          ▼
Publish Container Image
Checkout tested commit → Docker build → GHCR
latest + sha-<full commit SHA>
          │
          │ successful publish
          ▼
Deploy to Azure
GitHub OIDC → Microsoft Entra ID → Azure RBAC
          │
          ▼
Azure Container Apps
          │
          ▼
Azure SQL Database
```

### Continuous Integration

CI runs on pushes and pull requests targeting `master`. It restores the `.slnx` solution, builds it in Release configuration, and executes the complete test suite.

### Container Publishing

The publishing workflow runs only after a successful CI workflow on `master`. It explicitly checks out the commit identified by the completed CI run, then builds and publishes the API image to GitHub Container Registry.

Images receive both `latest` and `sha-<full commit SHA>` tags. The SHA tag creates a direct link between the tested source revision and the published image.

GHCR authentication uses GitHub's workflow-scoped `GITHUB_TOKEN` with `packages: write`; no separate registry PAT is stored.

### Deployment Automation

Deployment runs only after successful completion of the container-publishing workflow.

`azure/login` obtains an Azure token through GitHub OIDC federation with Microsoft Entra ID. The workflow has `id-token: write` permission and uses repository variables for non-secret Azure identifiers; no long-lived Azure client secret is stored in GitHub.

Azure RBAC grants the federated deployment identity the required Container Apps permissions. `az containerapp update` then deploys the exact image tagged with the originating workflow's full commit SHA.

This preserves traceability across the delivery chain:

```text
tested commit
    ↓
SHA-tagged container image
    ↓
OIDC-authenticated deployment
    ↓
Azure Container Apps
```

## Azure Deployment

The current cloud deployment uses:

- **Azure Container Apps** — API container hosting
- **Azure SQL Database** — relational persistence
- **GitHub Container Registry** — container image registry
- **Microsoft Entra ID + GitHub OIDC** — deployment authentication
- **Azure RBAC** — deployment authorization

Production configuration is supplied through Azure Container Apps environment variables and secret references. Development-data seeding and automatic startup migrations are disabled in Production; database migrations are applied deliberately rather than as a side effect of application startup.

The deployed application exposes separate liveness and SQL-backed readiness checks.

## Configuration and Secrets

Configuration follows ASP.NET Core's environment-based model:

- `appsettings.json` — non-sensitive defaults
- `appsettings.Development.json` — development-specific non-secret configuration
- .NET User Secrets — local application secrets
- `.env` — local Docker secrets, excluded from Git
- Azure Container Apps secret references — sensitive production configuration
- GitHub repository variables — non-secret Azure deployment identifiers
- GitHub OIDC — Azure deployment authentication without a stored client secret

Production connection strings and application passwords are not committed to the repository.

## Project Structure

```text
EnterpriseOperationsAPI/
├── EnterpriseOperations.Domain/
│   └── Entities/
├── EnterpriseOperations.Application/
│   ├── DTOs/
│   ├── Interfaces/
│   ├── Models/
│   ├── Services/
│   └── Settings/
├── EnterpriseOperations.Infrastructure/
│   ├── BackgroundJobs/
│   ├── Caching/
│   ├── Data/
│   ├── DependencyInjection/
│   ├── ExternalServices/
│   ├── Identity/
│   ├── Migrations/
│   └── Repositories/
├── EnterpriseOperations.API/
│   ├── Controllers/
│   ├── Extensions/
│   ├── Hangfire/
│   └── Middleware/
├── tests/
│   ├── EnterpriseOperations.UnitTests/
│   └── EnterpriseOperations.IntegrationTests/
├── .github/workflows/
├── docker-compose.yml
└── EnterpriseOperationsAPI.slnx
```

## Design Scope

The project is intentionally **production-oriented**, not presented as a complete production platform. It implements application and delivery patterns relevant to production services while keeping infrastructure scope appropriate for a portfolio system.

Capabilities such as private networking, centralized telemetry storage and alerting, external secret vaulting, high-availability/disaster-recovery strategy, and organization-specific operational controls would depend on the requirements of a real production environment.

## Project Status

**Complete.** The defined scope, automated test suite, container pipeline, and Azure deployment are implemented and operational.
