# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SuperPOS Backend is a complete Point of Sale (POS) system built with **.NET 10** following **Clean Architecture** and **Domain-Driven Design (DDD)** principles. The project serves as a comprehensive learning implementation covering advanced software development patterns and real-world features.

**Current Status**: Phase 1 Complete (100%) - All core features implemented including sales, inventory, returns, authentication, real-time chat, reports, and dashboard.

## Architecture

### Clean Architecture (4 Layers)

```
src/
├── Domain/           # Enterprise business rules (entities, value objects, domain events)
├── Application/      # Use cases, DTOs, CQRS handlers, specifications
├── Infrastructure/   # External concerns (EF Core, repositories, email, PDF generation)
└── Web.API/         # Controllers, middleware, configuration
```

**Dependency Rule**: Dependencies point inward. Domain has no dependencies. Application depends only on Domain. Infrastructure depends on Application. Web.API depends on all layers.

### Key Design Patterns

1. **CQRS (Command Query Responsibility Segregation)**
   - Commands: Write operations in `UseCases/*/CQRS/Commands/`
   - Queries: Read operations in `UseCases/*/CQRS/Queries/`
   - Custom Mediator implementation in `Application/DesignPatterns/Mediators/`
   - All handlers implement `IRequestHandler<TRequest, TResponse>`

2. **Repository Pattern**
   - Base interface: `Domain/Repositories/IRepositoryBase.cs`
   - Only aggregate roots have repositories (not internal entities like SaleDetail)
   - Repositories accessed via UnitOfWork: `IUnitOfWork.Products`, `IUnitOfWork.Sales`, etc.

3. **Unit of Work Pattern**
   - Interface: `Application/Interfaces/Persistence/IUnitOfWork.cs`
   - Coordinates transactions across multiple repositories
   - Call `SaveChangesAsync()` to persist all changes atomically
   - Supports explicit transactions: `BeginTransactionAsync()`, `CommitTransactionAsync()`, `RollbackTransactionAsync()`

4. **Specification Pattern**
   - Interface: `Domain/Specifications/ISpecification.cs`
   - Base class: `Domain/Specifications/BaseSpecification.cs`
   - Encapsulates complex queries with filtering, ordering, paging, and eager loading
   - Used with `Repository.ListAsync(specification)` and `Repository.CountAsync(specification)`
   - Specifications live in `Application/UseCases/*/Specifications/`

5. **Domain Events**
   - Interface: `Domain/Events/IDomainEvent.cs`
   - Events raised by entities (e.g., `SaleCreatedEvent`, `StockDecrementedEvent`)
   - Dispatched after `SaveChangesAsync()` by `DomainEventDispatcher`
   - Used for cross-aggregate communication (e.g., Sale → Inventory)

6. **Result Pattern**
   - `OperationResult<T>` for standardized responses
   - Contains `Data`, `StatusCode`, `Message`, `Errors`, `IsSuccess`
   - Avoids throwing exceptions for expected failures

### Entity Hierarchy

- **BaseEntity**: All entities inherit from this (Id, CreatedAt, UpdatedAt, DeletedAt for soft delete)
- **IAggregateRoot**: Marker interface for aggregate roots that can have repositories
- **BaseCatalog**: For simple catalog entities (Name, Description, IsActive)

## Common Development Commands

### Building and Running

```bash
# Build the solution
dotnet build

# Run the Web API (from src/Web.API/)
cd src/Web.API
dotnet run

# Watch mode (auto-reload on changes)
dotnet watch run
```

### Database Migrations

```bash
# Create a new migration (from repository root)
dotnet ef migrations add MigrationName --project src/Infrastructure --startup-project src/Web.API

# Apply migrations to database
dotnet ef database update --project src/Infrastructure --startup-project src/Web.API

# Remove last migration (if not applied)
dotnet ef migrations remove --project src/Infrastructure --startup-project src/Web.API

# Generate SQL script for migration
dotnet ef migrations script --project src/Infrastructure --startup-project src/Web.API
```

**Note**: Migrations auto-apply in Development mode on app startup via `MigrationsExtension.ApplyMigrations()`.

### Testing

```bash
# Run all tests
dotnet test

# Run tests in a specific project
dotnet test tests/Application.UnitTests

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Project Structure Conventions

### Adding a New Entity

1. **Domain Layer**:
   - Create entity in `Domain/Entities/{EntityName}/{EntityName}.cs`
   - Implement `IAggregateRoot` if it needs a repository
   - Add domain events in `Domain/Events/{EntityName}/`
   - Create repository interface in `Domain/Repositories/I{EntityName}Repository.cs`

2. **Infrastructure Layer**:
   - Implement repository in `Infrastructure/Persistence/Repositories/{EntityName}Repository.cs`
   - Add EF Core configuration in `Infrastructure/Persistence/Configurations/{EntityName}Configuration.cs`
   - Register in `SuperPOSDbContext.cs` and `UnitOfWork.cs`

3. **Application Layer**:
   - Create DTOs in `Application/UseCases/{EntityName}/DTOs/`
   - Add mappings in `Application/UseCases/{EntityName}/{EntityName}Mappings.cs` (using Mapster)
   - Create Commands in `Application/UseCases/{EntityName}/CQRS/Commands/`
   - Create Queries in `Application/UseCases/{EntityName}/CQRS/Queries/`
   - Add validators using FluentValidation if needed

4. **Web.API Layer**:
   - Create controller in `Web.API/Controllers/{EntityName}Controller.cs`
   - Controllers are thin - they only call mediator and return results

### CQRS Command/Query Structure

Each command/query follows this pattern:
```
UseCases/{Entity}/CQRS/Commands/{ActionName}/
├── {Entity}{Action}Command.cs        # The request (implements IRequest<T>)
└── {Entity}{Action}Handler.cs        # The handler (implements IRequestHandler<>)
```

Example: `UseCases/Products/CQRS/Commands/Create/ProductCreateCommand.cs`

## Important Architectural Rules

1. **Aggregate Boundaries**: Only aggregate roots can be accessed directly via repositories. Internal entities (like `SaleDetail`) must be accessed through their aggregate root (`Sale`).

2. **Soft Delete**: All entities have soft delete enabled. Use `Delete()` method which sets `DeletedAt`. Queries automatically exclude soft-deleted entities.

3. **Value Objects**: Use value objects for domain concepts with validation (Email, PhoneNumber, Barcode, Quantity, PersonName). They are immutable and enforce invariants.

4. **Domain Invariants**: Business rules are enforced in entity methods, not in services or handlers. Entities protect their own consistency.

5. **Two-Phase Commit**: For operations that need rollback capability (e.g., inventory reservation in sales):
   - Phase 1: Validate and reserve (`ValidateAndReserveStockAsync`)
   - Phase 2: Commit or rollback (`CommitReservationAsync` / `RollbackReservationAsync`)

6. **Custom Mediator**: This project uses a custom Mediator implementation, not MediatR. The API is similar but implementations are in `Application/DesignPatterns/Mediators/`.

## Configuration

- **appsettings.json**: Contains all application settings (JWT, email, business info, rate limiting)
- **User Secrets**: Use `dotnet user-secrets` for sensitive data (JWT secret key, email credentials, database connection strings)
- Connection string stored in user secrets: `ConnectionStrings:DefaultConnection`

## Key Technologies

- **.NET 10** (C# 13)
- **Entity Framework Core 10** with SQL Server
- **JWT Authentication** with refresh tokens and token rotation
- **SignalR** for real-time chat
- **QuestPDF** for PDF generation (tickets, reports)
- **ClosedXML** for Excel export
- **MailKit** for email notifications
- **BCrypt.Net** for password hashing
- **FluentValidation** for validation rules
- **Mapster** for object mapping
- **AspNetCoreRateLimit** for rate limiting

## Security Features

- JWT authentication with access and refresh tokens
- Refresh token rotation (new token issued on each refresh)
- Password complexity validation (min 8 chars, uppercase, lowercase, number, special char)
- Rate limiting on auth endpoints (5 login attempts/minute)
- Security headers middleware (XSS, clickjacking, MIME sniffing protection)
- Audit logging for security events
- Automatic token cleanup background service

## Additional Notes

- **Domain Events are dispatched AFTER SaveChangesAsync**: This ensures events only fire if the transaction succeeds.
- **Specifications vs Query methods**: Use specifications for complex queries with multiple criteria, includes, and paging. Use simple `QueryAsync()` for basic filtering.
- **Controllers use Mediator**: Controllers don't call repositories directly - they send commands/queries through the mediator.
- **PROJECT_PLAN.md and PROJECT_STATUS.md**: Comprehensive documentation of all features and implementation status. Do not modify these files unless explicitly requested.
