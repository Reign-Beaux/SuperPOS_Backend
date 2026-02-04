# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

```bash
# Run the application
dotnet run --project src/Web.API/Web.API.csproj

# Build the solution
dotnet build

# Restore packages
dotnet restore

# Create a new EF migration
dotnet ef migrations add <MigrationName> -p src/Infrastructure -s src/Web.API

# Apply migrations manually
dotnet ef database update -p src/Infrastructure -s src/Web.API
```

Migrations auto-apply on startup in development mode via `ApplyMigrations()`.

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

### Database Conventions

- All entities inherit from `BaseEntity` (Guid v7 ID, CreatedAt, UpdatedAt, DeletedAt)
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

- CORS origins required in `CorsSettings:Origins`
- User secrets enabled (ID: `7758d9ef-4f1e-495a-9803-32babd135164`)
- Connection string key: `SuperPOS`
