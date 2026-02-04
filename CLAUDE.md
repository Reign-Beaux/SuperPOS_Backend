# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

```bash
# Run the application
dotnet run --project src/Web.API/Web.API.csproj

# Run with hot reload (watch mode)
dotnet watch --project src/Web.API/Web.API.csproj

# Build the solution
dotnet build

# Restore packages
dotnet restore

# Run tests (if test projects exist)
dotnet test

# Create a new EF migration
dotnet ef migrations add <MigrationName> -p src/Infrastructure -s src/Web.API

# Apply migrations manually
dotnet ef database update -p src/Infrastructure -s src/Web.API
```

**Note:** Migrations auto-apply on startup in development mode via `ApplyMigrations()`.

## Architecture Overview

This is a .NET 10 / C# 13 REST API following **Clean Architecture** with four layers:

```
src/
├── Domain/          # Core entities, no external dependencies
├── Application/     # Use cases, CQRS, business logic
├── Infrastructure/  # EF Core, database, external services
└── Web.API/         # Controllers, HTTP entry point
```

**Dependency flow:** Web.API → Application ← Infrastructure → Domain

### Key Patterns

1. **Custom Mediator** (not MediatR) - `Application/DesignPatterns/Mediators/`
   - `IRequest<TResponse>` / `IRequestHandler<TRequest, TResponse>`
   - Supports pipeline behaviors via `IPipelineBehavior<,>`

2. **Result Pattern** - `Application/DesignPatterns/OperationResults/`
   - `OperationResult<T>` with `StatusResult` enum
   - Controllers use `HandleResult()` to map to HTTP responses
   - Factory methods in `Result` class: `Result.Ok()`, `Result.Error()`, `Result.NotFound()`

3. **CQRS per Entity** - `Application/UseCases/{Entity}/CQRS/`
   - Commands: `Create{Entity}Command`, `{Entity}UpdateCommand`, `{Entity}DeleteCommand`
   - Queries: `{Entity}GetByIdQuery`, `{Entity}GetAllQuery`
   - Each has a corresponding `Handler` class

4. **Repository + Unit of Work** - `Infrastructure/Persistence/`
   - Generic `Repository<T>` with soft delete support
   - `IUnitOfWork` for transaction management

5. **Specification Pattern** - `Application/DesignPatterns/Specifications/`
   - `ISpecification<T>` / `BaseSpecification<T>` for complex queries
   - Supports: Criteria, Includes, OrderBy/OrderByDescending, Pagination
   - `SpecificationEvaluator` in Infrastructure applies specifications to IQueryable
   - `BasePaginationQuery` and `PaginationDTO` for paginated responses

### Database Conventions

- All entities inherit from `BaseEntity` (Guid v7 ID, CreatedAt, UpdatedAt, DeletedAt)
- Catalog entities inherit from `BaseCatalog` (extends BaseEntity with Name, Description)
- Soft deletes: `DeletedAt` is set instead of removing rows
- Connection string: `SuperPOS` in configuration
- SQL Server via EF Core 10

## Adding a New Entity

1. **Domain**: Create entity in `src/Domain/Entities/{Entity}/`
2. **Application**:
   - Create folder `src/Application/UseCases/{Entity}/`
   - Add `DTOs/{Entity}DTO.cs`
   - Add `{Entity}Mappings.cs` (Mapster config)
   - Add `{Entity}Rules.cs` (business validation)
   - Add `CQRS/Commands/` and `CQRS/Queries/` with handlers
3. **Infrastructure**:
   - Add `DbSet<Entity>` to `SuperPOSDbContext`
   - Add EF configuration in `Persistence/Configurations/`
   - Create migration
4. **Web.API**: Add controller inheriting `BaseController`

## Naming Conventions

- Commands: `Create{Entity}Command`, `{Entity}UpdateCommand`, `{Entity}DeleteCommand`
- Queries: `{Entity}GetByIdQuery`, `{Entity}GetAllQuery`
- Handlers: `Create{Entity}Handler`, `{Entity}GetByIdHandler`, etc.
- DTOs: `{Entity}DTO`
- Rules: `{Entity}Rules`
- Messages: `{Entity}Messages` (in Domain)

## Configuration

### Required Settings

**appsettings.json** (minimum configuration):
```json
{
  "ConnectionStrings": {
    "SuperPOS": "Server=.;Database=SuperPOS;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "CorsSettings": {
    "Origins": ["http://localhost:3000"]
  }
}
```

### User Secrets
- User secrets ID: `7758d9ef-4f1e-495a-9803-32babd135164`
- Store sensitive data (connection strings, API keys) using:
  ```bash
  dotnet user-secrets set "ConnectionStrings:SuperPOS" "your-connection-string"
  ```

### Environment Variables
All configuration can be overridden via environment variables using the format:
- `ConnectionStrings__SuperPOS`
- `CorsSettings__Origins__0`

## Dependency Injection

Each layer registers its services via extension methods:

### Web.API Layer - `AddWebAPI()`
- Controllers
- OpenAPI documentation
- JSON serialization (with support for Value Object converters)
- `GlobalExceptionHandlingMiddleware` (handles unhandled exceptions, returns ProblemDetails)

### Application Layer - `AddApplication()`
- Custom Mediator with all handlers from assembly
- FluentValidation validators
- Mapster configuration (auto-scanned from assembly)
- Pipeline behaviors (placeholder for cross-cutting concerns)

### Infrastructure Layer - `AddInfrastructure()`
- DbContext (`SuperPOSDbContext`) with SQL Server
- `IUnitOfWork` and `Repository<T>`
- `IEncryptionService` (for password hashing, etc.)
- Caching (placeholder for future implementation)

## Mapster Configuration

Mapster is used for object mapping. Configuration per entity:

**Location:** `Application/UseCases/{Entity}/{Entity}Mappings.cs`

**Example:**
```csharp
public class CustomerMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Customer, CustomerDTO>();
        config.NewConfig<CreateCustomerCommand, Customer>();
        config.NewConfig<CustomerUpdateCommand, Customer>();
    }
}
```

Mapster auto-scans the Application assembly and registers all `IRegister` implementations.

**Usage in handlers:**
```csharp
var dto = customer.Adapt<CustomerDTO>();
```

## API Documentation

### OpenAPI/Swagger
- Available in **development mode only**
- Endpoint: `/openapi/v1.json`
- Interactive UI: Not configured (add Swagger UI if needed)

### Controllers
All controllers inherit from `BaseController` which provides:
- `[ApiController]` attribute
- `HandleResult<T>()` method that maps `OperationResult<T>` to HTTP responses:
  - `StatusResult.Ok` → 200 OK
  - `StatusResult.Created` → 201 Created (with Location header)
  - `StatusResult.NoContent` → 204 No Content
  - `StatusResult.NotFound` → 404 Not Found
  - `StatusResult.BadRequest` → 400 Bad Request
  - `StatusResult.Conflict` → 409 Conflict
  - `StatusResult.Exists` → 409 Conflict

## Error Handling

Two-level error handling strategy:

1. **Business Logic Errors** - Use `OperationResult<T>` pattern
   - Return `Result.Error()`, `Result.NotFound()`, etc.
   - Controllers map to appropriate HTTP status codes

2. **Unhandled Exceptions** - `GlobalExceptionHandlingMiddleware`
   - Catches all unhandled exceptions
   - Returns 500 Internal Server Error with ProblemDetails
   - Logs exception message (configured in middleware)

## Advanced Patterns

### Specification Pattern Usage

Create specifications for complex queries:

```csharp
public class CustomerActiveSpecification : BaseSpecification<Customer>
{
    public CustomerActiveSpecification() : base(c => c.DeletedAt == null)
    {
        AddInclude(c => c.Orders);
        AddOrderBy(c => c.Name);
    }
}
```

Use in repository:
```csharp
var customers = await _repository.GetAllAsync(new CustomerActiveSpecification());
```

### Pagination

Use `BasePaginationQuery` for paginated endpoints:
```csharp
public record CustomersGetPagedQuery(int PageNumber, int PageSize) : BasePaginationQuery;
```

Return `PaginationDTO<T>` with metadata (page, size, total count).

## Services

### IEncryptionService
Located in `Application/Interfaces/Services/`, implemented in Infrastructure.

**Purpose:** Password hashing, data encryption (implementation details in Infrastructure layer)
