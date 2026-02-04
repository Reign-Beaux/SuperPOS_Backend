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

### Global Usings

Each layer has a `GlobalUsings.cs` file that provides commonly-used namespaces globally. **DO NOT** add these usings to individual files as they are already available:

#### Web.API Layer (`src/Web.API/GlobalUsings.cs`)
```csharp
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
```

#### Application Layer (`src/Application/GlobalUsings.cs`)
```csharp
global using FluentValidation;
global using Mapster;
global using MapsterMapper;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;
global using System.Linq.Expressions;
```

#### Infrastructure Layer (`src/Infrastructure/GlobalUsings.cs`)
```csharp
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore.Storage;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using System.Linq.Expressions;
```

**Important:** When creating new files, only add using statements for namespaces NOT in the global usings list.

#### Required Usings by File Type

**Commands/Queries** (Application layer):
```csharp
using Application.DesignPatterns.Mediators.Interfaces;  // IRequest<T>
using Application.DesignPatterns.OperationResults;      // OperationResult<T>
using Application.UseCases.{Entity}.DTOs;               // DTOs
// Note: MapsterMapper, System.Linq.Expressions already global
```

**Handlers** (Application layer):
```csharp
using Application.DesignPatterns.Mediators.Interfaces;  // IRequestHandler<,>
using Application.DesignPatterns.OperationResults;      // OperationResult<T>, Result
using Application.Interfaces.Persistence.UnitOfWorks;   // IUnitOfWork
using Application.UseCases.{Entity}.DTOs;               // DTOs
using Domain.Entities.{Entity};                         // Entity classes
// Note: MapsterMapper already global (provides IMapper)
```

**Controllers** (Web.API layer):
```csharp
using Application.DesignPatterns.Mediators.Interfaces;  // IMediator
using Application.UseCases.{Entity}.CQRS.Commands.*;    // Commands
using Application.UseCases.{Entity}.CQRS.Queries.*;     // Queries
// Note: Microsoft.AspNetCore.Mvc already global
```

**Rules** (Application layer):
```csharp
using Application.Interfaces.Persistence.UnitOfWorks;   // IUnitOfWork
using Domain.Entities.{Entity};                         // Entity classes
// Note: System.Linq.Expressions already global
```

**Mappings** (Application layer):
```csharp
using Application.UseCases.{Entity}.DTOs;               // DTOs
using Domain.Entities.{Entity};                         // Entity classes
// Note: Mapster already global (provides TypeAdapterConfig, IRegister)
```

**Entity Configurations** (Infrastructure layer):
```csharp
using Domain.Entities.{Entity};                         // Entity classes
// Note: EntityFrameworkCore.Metadata.Builders already global (provides IEntityTypeConfiguration)
```

### Key Patterns

1. **Custom Mediator** (not MediatR) - `Application/DesignPatterns/Mediators/`
   - `IRequest<TResponse>` / `IRequestHandler<TRequest, TResponse>`
   - Supports pipeline behaviors via `IPipelineBehavior<,>`

2. **Result Pattern** - `Application/DesignPatterns/OperationResults/`
   - `OperationResult<T>` with `StatusResult` enum
   - Controllers use `HandleResult()` to map to HTTP responses
   - Factory methods in `Result` class:
     - `Result.Success<T>(T data)` - Returns 200 OK
     - `Result.Success<T>(T data, string message)` - Returns 200 OK with message
     - `Result.Error<T>(ErrorResult errorType, string? detail = null)` - Returns error (400/404/409 based on ErrorResult)
     - `new OperationResult<T>(StatusResult.Created, T data)` - Returns 201 Created
   - `ErrorResult` enum values:
     - `ErrorResult.BadRequest` → 400 Bad Request
     - `ErrorResult.NotFound` → 404 Not Found
     - `ErrorResult.Conflict` → 409 Conflict

3. **CQRS per Entity** - `Application/UseCases/{Entity}/CQRS/`
   - Commands: `Create{Entity}Command`, `{Entity}UpdateCommand`, `{Entity}DeleteCommand`
   - Queries: `{Entity}GetByIdQuery`, `{Entity}GetAllQuery`
   - Each has a corresponding `Handler` class

4. **Repository + Unit of Work** - `Infrastructure/Persistence/`
   - Generic `Repository<T>` with soft delete support
   - `IUnitOfWork` for transaction management
   - Repository methods available:
     - `GetAllAsync(CancellationToken)` - Get all entities (excludes soft-deleted)
     - `GetByIdAsync(Guid id, CancellationToken)` - Get entity by ID
     - `Add(T entity)` - Add entity (call SaveChangesAsync to persist)
     - `Update(T entity)` - Update entity (call SaveChangesAsync to persist)
     - `Delete(T entity)` - Soft delete entity (call SaveChangesAsync to persist)
     - `FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken)` - Find first match
     - `QueryAsync(Expression<Func<T, bool>>? predicate, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy, CancellationToken)` - Query with filters
     - `GetAllWithSpecAsync(ISpecification<T> spec, CancellationToken)` - Query using specification pattern
     - `CountAsync(ISpecification<T> spec, CancellationToken)` - Count using specification
   - Access via: `_unitOfWork.Repository<EntityType>()`
   - Note: Repository does NOT support eager loading (includes). Load related entities manually or use specifications

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

### Loading Related Entities

Since the Repository does not support eager loading (`.Include()`), you must load related entities manually:

```csharp
// Get the main entity
var sale = await _unitOfWork.Repository<Sale>().GetByIdAsync(saleId, cancellationToken);

// Load related entities manually
sale.Customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(sale.CustomerId, cancellationToken);
sale.User = await _unitOfWork.Repository<User>().GetByIdAsync(sale.UserId, cancellationToken);

// Load collection navigation properties
var saleDetails = await _unitOfWork.Repository<SaleDetail>().QueryAsync(
    sd => sd.SaleId == sale.Id,
    cancellationToken: cancellationToken
);
sale.SaleDetails = saleDetails.ToList();

// Load nested relations
foreach (var detail in sale.SaleDetails)
{
    detail.Product = await _unitOfWork.Repository<Product>().GetByIdAsync(detail.ProductId, cancellationToken);
}
```

**Alternative:** For complex queries with multiple includes, create a Specification.

### Specification Pattern Usage

Create specifications for complex queries with eager loading:

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
var customers = await _repository.GetAllWithSpecAsync(new CustomerActiveSpecification());
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

## Complete Implementation Examples

### Minimal Handler Example (Query)

```csharp
// File: Application/UseCases/Products/CQRS/Queries/GetById/ProductGetByIdQuery.cs
using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Products.DTOs;

namespace Application.UseCases.Products.CQRS.Queries.GetById;

public record ProductGetByIdQuery(Guid Id) : IRequest<OperationResult<ProductDTO>>;
```

```csharp
// File: Application/UseCases/Products/CQRS/Queries/GetById/ProductGetByIdHandler.cs
using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Application.UseCases.Products.DTOs;
using Domain.Entities.Products;

namespace Application.UseCases.Products.CQRS.Queries.GetById;

public class ProductGetByIdHandler : IRequestHandler<ProductGetByIdQuery, OperationResult<ProductDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductGetByIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OperationResult<ProductDTO>> Handle(ProductGetByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(request.Id, cancellationToken);

        if (product == null)
            return Result.Error<ProductDTO>(ErrorResult.NotFound, detail: ProductMessages.NotFound.WithId(request.Id));

        var dto = _mapper.Map<ProductDTO>(product);
        return Result.Success(dto);
    }
}
```

### Handler with Manual Relationship Loading

```csharp
// File: Application/UseCases/Sales/CQRS/Queries/GetById/SaleGetByIdHandler.cs
using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Application.UseCases.Sales.DTOs;
using Domain.Entities.Customers;
using Domain.Entities.Sales;
using Domain.Entities.Users;

namespace Application.UseCases.Sales.CQRS.Queries.GetById;

public class SaleGetByIdHandler : IRequestHandler<SaleGetByIdQuery, OperationResult<SaleDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SaleGetByIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OperationResult<SaleDTO>> Handle(SaleGetByIdQuery request, CancellationToken cancellationToken)
    {
        var sale = await _unitOfWork.Repository<Sale>().GetByIdAsync(request.Id, cancellationToken);
        if (sale == null)
            return Result.Error<SaleDTO>(ErrorResult.NotFound, detail: SaleMessages.NotFound.WithId(request.Id));

        // Load relationships manually
        sale.Customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(sale.CustomerId, cancellationToken);
        sale.User = await _unitOfWork.Repository<User>().GetByIdAsync(sale.UserId, cancellationToken);

        // Load collection
        var saleDetails = await _unitOfWork.Repository<SaleDetail>().QueryAsync(
            sd => sd.SaleId == sale.Id,
            cancellationToken: cancellationToken
        );
        sale.SaleDetails = saleDetails.ToList();

        // Load nested relationships
        foreach (var detail in sale.SaleDetails)
        {
            detail.Product = await _unitOfWork.Repository<Product>().GetByIdAsync(detail.ProductId, cancellationToken);
        }

        var dto = _mapper.Map<SaleDTO>(sale);
        return Result.Success(dto);
    }
}
```

### Controller Example

```csharp
// File: Web.API/Controllers/ProductController.cs
using Application.DesignPatterns.Mediators.Interfaces;
using Application.UseCases.Products.CQRS.Commands.Create;
using Application.UseCases.Products.CQRS.Queries.GetById;
using Application.UseCases.Products.CQRS.Queries.GetAll;

namespace Web.API.Controllers;

[Route("api/[controller]")]
public class ProductController : BaseController
{
    private readonly IMediator _mediator;

    public ProductController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new ProductGetByIdQuery(id);
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new ProductGetAllQuery();
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }
}
```
