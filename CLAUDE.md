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
     - `Result.Error(ErrorResult errorType, string? detail = null)` - Returns error (returns OperationResult<VoidResult>)
     - `new OperationResult<T>(StatusResult.Created, T data)` - Returns 201 Created
   - `StatusResult` enum values:
     - `Ok`, `NoContent`, `Created`, `Exists`, `Conflict`, `BadRequest`, `NotFound`, `InternalServerError`, `Forbidden`, `ServiceUnavailable`, `GatewayTimeout`
   - `ErrorResult` enum values (subset of StatusResult for errors):
     - `ErrorResult.BadRequest` → 400 Bad Request
     - `ErrorResult.NotFound` → 404 Not Found
     - `ErrorResult.Conflict` → 409 Conflict

3. **CQRS per Entity** - `Application/UseCases/{Entity}/CQRS/`
   - Commands: `Create{Entity}Command`, `{Entity}UpdateCommand`, `{Entity}DeleteCommand`
   - Queries: `{Entity}GetByIdQuery`, `{Entity}GetAllQuery`
   - Each has a corresponding `Handler` class

4. **Repository + Unit of Work** - `Infrastructure/Persistence/`
   - Generic `RepositoryBase<T>` with soft delete support
   - `IUnitOfWork` for transaction management
   - Base repository methods available:
     - `GetAllAsync(CancellationToken)` - Get all entities (excludes soft-deleted)
     - `GetByIdAsync(Guid id, CancellationToken)` - Get entity by ID
     - `Add(T entity)` - Add entity (call SaveChangesAsync to persist)
     - `Update(T entity)` - Update entity (call SaveChangesAsync to persist)
     - `Delete(T entity)` - Soft delete entity (call SaveChangesAsync to persist)
     - `FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken)` - Find first match
     - `QueryAsync(Expression<Func<T, bool>>? predicate, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy, CancellationToken)` - Query with filters
     - `AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken)` - Check if any match
     - `CountAsync(Expression<Func<T, bool>>? predicate, CancellationToken)` - Count entities
     - `GetAllWithSpecAsync(ISpecification<T> spec, CancellationToken)` - Query using specification pattern
   - **UnitOfWork specific repositories** (preferred over generic):
     - `_unitOfWork.Products` - IProductRepository (with SearchByNameAsync, ExistsByBarcodeAsync, etc.)
     - `_unitOfWork.Customers` - ICustomerRepository (with SearchByNameAsync)
     - `_unitOfWork.Users` - IUserRepository (with SearchByNameAsync)
     - `_unitOfWork.Sales` - ISaleRepository (with GetByDateRangeAsync, GetByIdWithDetailsAsync, etc.)
     - `_unitOfWork.Inventories` - IInventoryRepository (with GetByProductIdAsync, GetLowStockItemsAsync)
     - `_unitOfWork.Returns` - IReturnRepository (with GetByStatusAsync, GetBySaleIdAsync, etc.)
     - `_unitOfWork.CashRegisters` - ICashRegisterRepository (with GetByIdWithDetailsAsync)
     - `_unitOfWork.Roles` - IRoleRepository
   - Generic access: `_unitOfWork.Repository<EntityType>()`
   - **Note:** Base repository does NOT support eager loading. However, specific repositories (like ISaleRepository) have custom methods with eager loading (e.g., `GetByIdWithDetailsAsync()`)

5. **Specification Pattern** - `Application/DesignPatterns/Specifications/`
   - `ISpecification<T>` / `BaseSpecification<T>` for complex queries
   - Supports: Criteria, Includes, OrderBy/OrderByDescending, Pagination
   - `SpecificationEvaluator` in Infrastructure applies specifications to IQueryable
   - `BasePaginationQuery` and `PaginationDTO` for paginated responses
   - **Note:** Infrastructure exists but no actual specifications are currently implemented. Use specific repository methods for now.

### Database Conventions

- All entities inherit from `BaseEntity` (Guid v7 ID, CreatedAt, UpdatedAt, DeletedAt)
- Catalog entities inherit from `BaseCatalog` (extends BaseEntity with Name, Description?)
  - **Note:** `Description` is nullable (string?) in `BaseCatalog` as of 2026-02-11
- Soft deletes: `DeletedAt` is set instead of removing rows
- Connection string: `SuperPOS` in configuration
- SQL Server via EF Core 10

### Nullable Reference Types

**Status**: ✅ Enabled in all projects (`<Nullable>enable</Nullable>`)

**Important Guidelines**:
- ✅ All nullable warnings have been resolved (as of 2026-02-11)
- ❌ **Never use null-forgiving operator (`!`)** without null validation
- ✅ Always validate null before assignment when using `FirstOrDefaultAsync()`
- ✅ Use `string?` for optional parameters
- ✅ Use null-coalescing operator (`??`) for default values

**Common Patterns**:

```csharp
// ❌ WRONG - Null-forgiving without validation
user.Role = (await _context.Set<Role>().FirstOrDefaultAsync(...))!;

// ✅ CORRECT - Validate before assignment
var role = await _context.Set<Role>().FirstOrDefaultAsync(...);
if (role != null)
    user.Role = role;

// ✅ CORRECT - Nullable parameter with default
public static string WithName(string? value) =>
    $"Product with name '{value ?? "unknown"}' already exists";

// ✅ CORRECT - Argument validation for non-nullable parameters
public async Task SendEmailAsync(string recipientEmail, ...)
{
    if (string.IsNullOrWhiteSpace(recipientEmail))
        throw new ArgumentException("Recipient email cannot be null or empty", nameof(recipientEmail));
    // ...
}
```

## Adding a New Entity

1. **Domain**:
   - Create entity in `src/Domain/Entities/{Entity}/`
   - Add domain messages in `{Entity}Messages.cs`
   - Add repository interface `I{Entity}Repository` in `src/Domain/Repositories/` if custom methods needed
   - Consider value objects if needed
   - Add domain events if needed
2. **Application**:
   - Create folder `src/Application/UseCases/{Entity}/`
   - Add `DTOs/{Entity}DTO.cs`
   - Add `{Entity}Mappings.cs` (Mapster config)
   - Add `CQRS/Commands/` and `CQRS/Queries/` with handlers
   - **Validation:** Implement in handlers directly or via domain services (no separate validator files)
3. **Infrastructure**:
   - Add `DbSet<Entity>` to `SuperPOSDbContext`
   - Add EF configuration in `Persistence/Configurations/`
   - Add repository implementation in `Persistence/Repositories/` if custom methods needed
   - Add repository property to `IUnitOfWork` and `UnitOfWork`
   - Create migration
4. **Web.API**:
   - Add controller inheriting `BaseController`
   - **Important:** POST endpoints must call `HandleResult(result, nameof(GetById))` to generate Location header

## Naming Conventions

- Commands: `Create{Entity}Command`, `{Entity}UpdateCommand`, `{Entity}DeleteCommand`
- Queries: `{Entity}GetByIdQuery`, `{Entity}GetAllQuery`, `{Entity}SearchQuery`
- Handlers: `Create{Entity}Handler`, `{Entity}GetByIdHandler`, etc.
- DTOs: `{Entity}DTO` (use record types)
- Messages: `{Entity}Messages` (in Domain, Spanish language)
- Repositories: `I{Entity}Repository` (interface in Domain), `{Entity}Repository` (implementation in Infrastructure)
- Mappings: `{Entity}Mappings` (implements IRegister)

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
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "noreply@superpos.com",
    "SenderName": "SuperPOS System",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "EnableSsl": true
  },
  "StockSettings": {
    "LowStockThreshold": 10
  },
  "BusinessInfo": {
    "Name": "Super POS",
    "Address": "Av. Principal #123, Col. Centro, Ciudad",
    "Phone": "(555) 123-4567",
    "Email": "contacto@superpos.com",
    "RFC": "XAXX010101000",
    "TaxRegime": "General de Ley Personas Morales"
  }
}
```

**Notes:**
- `EmailSettings`: Required for low stock alert emails (uses MailKit/SMTP)
- `StockSettings.LowStockThreshold`: Triggers email alerts when stock ≤ this value (default: 10)
- `BusinessInfo`: Required for PDF ticket generation (business details on tickets and reports)

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

### External NuGet Packages

**Core Packages** (already installed):
- Entity Framework Core 10.x
- Mapster / MapsterMapper
- Microsoft.Extensions.*

**Feature-Specific Packages**:
- **MailKit 4.14.1** - SMTP email sending (low stock alerts)
- **MimeKit 4.14.0** - Email message composition (required by MailKit)
- **QuestPDF 2025.12.4** - PDF generation (tickets and reports)
  - Uses Community License (free for non-commercial use)
  - For production commercial use, acquire appropriate license

## Dependency Injection

Each layer registers its services via extension methods:

### Web.API Layer - `AddWebAPI()`
- Controllers
- OpenAPI documentation
- JSON serialization (with support for Value Object converters)
- `GlobalExceptionHandlingMiddleware` (handles unhandled exceptions, returns ProblemDetails)

### Application Layer - `AddApplication()`
- Custom Mediator with all handlers from assembly
- Mapster configuration (auto-scanned from assembly)
- Domain event dispatcher
- Pipeline behaviors (placeholder for cross-cutting concerns)

### Infrastructure Layer - `AddInfrastructure()`
- DbContext (`SuperPOSDbContext`) with SQL Server
- `IUnitOfWork` and all repository implementations (Products, Customers, Users, Sales, Inventories, CashRegisters, Returns, Roles, EmailLogs)
- Domain services implementations:
  - `IProductUniquenessChecker`, `ICustomerUniquenessChecker`, `IUserUniquenessChecker`
  - `ISaleValidationService` (validates customer/user existence)
  - `IStockReservationService` (two-phase stock reservation with commit/rollback)
- Application services:
  - `IEmailService` (MailKit email notifications with HTML templates)
  - `ITicketService` (QuestPDF ticket and report generation)
- `IEncryptionService` (password hashing)
- `IDomainEventDispatcher` (dispatches domain events to handlers)
- Event handlers:
  - `LowStockEventHandler` (sends email alerts)
  - `SaleCancelledEventHandler` (restores inventory)
  - `ReturnApprovedEventHandler` (restores inventory)
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

**Option 1: Use Specific Repository Methods (Recommended)**

Some repositories have custom methods with eager loading:

```csharp
// Sale repository has methods with eager loading
var sale = await _unitOfWork.Sales.GetByIdWithDetailsAsync(saleId, cancellationToken);
// Returns Sale with Customer, User, SaleDetails, and Products loaded

var allSales = await _unitOfWork.Sales.GetAllWithDetailsAsync(cancellationToken);
// Returns all Sales with complete details

var salesByDate = await _unitOfWork.Sales.GetByDateRangeAsync(startDate, endDate, cancellationToken);
// Returns Sales filtered by date range
```

**Option 2: Manual Loading (if specific method doesn't exist)**

Since base repository does not support eager loading, load related entities manually:

```csharp
// Get the main entity
var sale = await _unitOfWork.Sales.GetByIdAsync(saleId, cancellationToken);

// Load related entities manually
sale.Customer = await _unitOfWork.Customers.GetByIdAsync(sale.CustomerId, cancellationToken);
sale.User = await _unitOfWork.Users.GetByIdAsync(sale.UserId, cancellationToken);

// Load collection navigation properties
var saleDetails = await _unitOfWork.Repository<SaleDetail>().QueryAsync(
    sd => sd.SaleId == sale.Id,
    cancellationToken: cancellationToken
);
sale.SaleDetails = saleDetails.ToList();

// Load nested relations
foreach (var detail in sale.SaleDetails)
{
    detail.Product = await _unitOfWork.Products.GetByIdAsync(detail.ProductId, cancellationToken);
}
```

**Best Practice:** Check if a specific repository method exists before implementing manual loading.

### Specification Pattern Usage (Future Feature)

**Note:** Specification pattern infrastructure exists but is not currently used. The pattern is available for future implementation.

If you need to create a specification:

```csharp
public class CustomerActiveSpecification : BaseSpecification<Customer>
{
    public CustomerActiveSpecification() : base(c => c.DeletedAt == null)
    {
        AddInclude(c => c.Sales);
        AddOrderBy(c => c.Name);
    }
}
```

Use in repository:
```csharp
var customers = await _unitOfWork.Repository<Customer>().GetAllWithSpecAsync(new CustomerActiveSpecification());
```

**Current Approach:** Use specific repository methods or manual filtering instead.

### Pagination (Future Feature)

**Note:** Pagination infrastructure exists (`BasePaginationQuery`, `PaginationDTO<T>`) but is not currently used.

When implementing pagination:
```csharp
public record CustomersGetPagedQuery(int PageNumber, int PageSize) : BasePaginationQuery;
```

Return `PaginationDTO<T>` with metadata (page, size, total count, page count).

## Services

### IEncryptionService
Located in `Application/Interfaces/Services/`, implemented in Infrastructure.

**Purpose:** Password hashing, data encryption (implementation details in Infrastructure layer)

### Domain Services

**Purpose:** Encapsulate domain logic that doesn't belong to a single entity. All interfaces are in `Domain/Services/`, implementations in `Infrastructure/Services/Domain/`.

- **IProductUniquenessChecker** - Validates product name and barcode uniqueness
- **ICustomerUniquenessChecker** - Validates customer uniqueness
- **IUserUniquenessChecker** - Validates user uniqueness (email)
- **ISaleValidationService** - Validates sales (customer/user existence)
- **IStockReservationService** - Two-phase stock reservation:
  - `ValidateAndReserveStockAsync()` - Phase 1: Validate and reserve
  - `CommitReservationAsync()` - Phase 2: Commit changes
  - `RollbackReservationAsync()` - Rollback if sale creation fails

### Application Services

**Purpose:** Application-level services for cross-cutting concerns. Interfaces in `Application/Interfaces/Services/`, implementations in `Infrastructure/Services/`.

- **IEmailService** - Email notifications using MailKit/MimeKit
  - `SendLowStockAlertAsync()` - Sends HTML email alerts to managers when stock ≤ threshold
  - Logs all email attempts to `EmailLog` table
  - Configuration via `EmailSettings` in appsettings.json

- **ITicketService** - PDF generation using QuestPDF
  - `GenerateSaleTicketAsync()` - Generates professional sale tickets with business info, products, subtotal, IVA (16%), total
  - `GenerateCashRegisterReportAsync()` - Generates cash register reports with financial summary, statistics, and sales detail
  - Configuration via `BusinessInfo` in appsettings.json

## Current Project State

### ✅ Fully Implemented Features

1. **Product Management** - CRUD, search by name/barcode, barcode uniqueness
2. **Customer Management** - CRUD, search by name
3. **User Management** - CRUD, search by name with role, password hashing
4. **Role Management** - CRUD operations
5. **Inventory Management** - Stock adjustment (Add/Set/Remove operations), automatic restoration on sale cancellation/return approval
6. **Sales Management** - Create sales with stock reservation and validation, sale cancellation with inventory rollback, PDF ticket generation
7. **Cash Register** - Create cash register close, automatic sales calculation, PDF report generation
8. **Returns & Exchanges** - Complete return workflow (create, approve, reject), 30-day window validation, automatic inventory restoration
9. **Email Notifications** - Low stock alerts sent to managers (MailKit), email logging (EmailLog entity)
10. **PDF Generation** - Professional tickets and cash register reports (QuestPDF)
11. **Search Functionality** - Products, Customers, and Users
12. **Soft Delete** - All entities support soft delete with timestamp
13. **Value Objects** - Email, PersonName, PhoneNumber, Barcode, Quantity (Money was removed)
14. **Domain Events** - Product created/price changed, sale created/cancelled, low stock detected, return approved
15. **Two-Phase Stock Reservation** - Prevents overselling with validate/reserve, commit/rollback pattern
16. **Nullable Reference Types** - All projects have nullable enabled, all warnings resolved (2026-02-11)

### ⚠️ Implemented Infrastructure, Not Used

- **Pagination** - `BasePaginationQuery` and `PaginationDTO` exist but no queries use them
- **Specification Pattern** - Infrastructure exists but no actual specifications implemented

### ❌ Not Implemented

1. **Payment Methods** - Sales do not track payment type (cash, card, etc.)
2. **Authentication/Authorization** - No JWT or auth middleware
3. **Product Categories** - No product categorization
4. **Unit Tests** - No test projects exist
5. **Caching** - Placeholder only
6. **Sale Updates** - Sales can be cancelled but not edited (immutable after creation)

### Important Notes

- **Validation:** No FluentValidation or separate validator files. Validation is done:
  - In domain entities via value objects and factory methods
  - In handlers directly
  - Via domain services (uniqueness checkers, validation services)
- **Messages:** All user-facing messages are in Spanish
- **Specific Repositories:** Some entities (Sales, Products, etc.) have specialized repository methods. Always check `IUnitOfWork` properties before using generic `Repository<T>()`
- **CreatedAtAction:** All POST endpoints that create resources must pass `nameof(GetById)` to `HandleResult()` for proper Location header generation
- **Domain Events:** Event-driven architecture in place:
  - Events raised by aggregate roots (Sale, Inventory, Return)
  - Event handlers registered in DI and executed automatically on SaveChangesAsync
  - Examples: LowStockEvent → email alerts, SaleCancelledEvent → inventory restoration, ReturnApprovedEvent → inventory restoration
- **Immutable Sales:** Sales cannot be edited after creation, only cancelled (with automatic inventory rollback)
- **Return Window:** Returns must be requested within 30 days of the original sale
- **Email Configuration:** Requires valid SMTP credentials in EmailSettings. Use Gmail app passwords for development.
- **PDF License:** QuestPDF Community License in use. For commercial production, acquire appropriate license.
- **Nullable References:** All projects have `<Nullable>enable</Nullable>`. Never use `!` operator without null validation. Always validate `FirstOrDefaultAsync()` results before assignment.
- **BaseCatalog.Description:** Changed from `string` to `string?` (nullable) as of migration `FixNullableDescriptions` (2026-02-11).

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
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);

        if (product == null)
            return Result.Error(ErrorResult.NotFound, detail: ProductMessages.NotFound.WithId(request.Id));

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
        // Use specific repository method with eager loading instead of manual loading
        var sale = await _unitOfWork.Sales.GetByIdWithDetailsAsync(request.Id, cancellationToken);

        if (sale == null)
            return Result.Error(ErrorResult.NotFound, detail: SaleMessages.NotFound.WithId(request.Id));

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
        return HandleResult(result, nameof(GetById));
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
