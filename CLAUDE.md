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

# Run tests
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
├── Domain/          # Core entities, value objects, domain events, interfaces
├── Application/     # Use cases, CQRS, DTOs, handlers, business logic
├── Infrastructure/  # EF Core, database, repositories, external services
└── Web.API/         # Controllers, middleware, HTTP entry point
```

**Dependency flow:** Web.API → Application ← Infrastructure → Domain

### Global Usings

Each layer has a `GlobalUsings.cs` file that provides commonly-used namespaces globally. **DO NOT** add these usings to individual files as they are already available:

#### Web.API Layer
```csharp
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
```

#### Application Layer
```csharp
global using Application.Interfaces.Persistence;
global using FluentValidation;
global using Mapster;
global using MapsterMapper;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;
global using System.Linq.Expressions;
```

#### Infrastructure Layer
```csharp
global using Application.Interfaces.Persistence;
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
```

**Handlers** (Application layer):
```csharp
using Application.DesignPatterns.Mediators.Interfaces;  // IRequestHandler<,>
using Application.DesignPatterns.OperationResults;      // OperationResult<T>, Result
using Application.Interfaces.Persistence.UnitOfWorks;   // IUnitOfWork
using Application.UseCases.{Entity}.DTOs;               // DTOs
using Domain.Entities.{Entity};                         // Entity classes
```

**Controllers** (Web.API layer):
```csharp
using Application.DesignPatterns.Mediators.Interfaces;  // IMediator
using Application.UseCases.{Entity}.CQRS.Commands.*;    // Commands
using Application.UseCases.{Entity}.CQRS.Queries.*;     // Queries
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
// Note: EntityFrameworkCore.Metadata.Builders already global
```

## Key Patterns

1. **Custom Mediator** (not MediatR) - `Application/DesignPatterns/Mediators/`
   - `IRequest<TResponse>` / `IRequestHandler<TRequest, TResponse>`
   - Supports pipeline behaviors via `IPipelineBehavior<,>`

2. **Result Pattern** - `Application/DesignPatterns/OperationResults/`
   - `OperationResult<T>` with `StatusResult` enum
   - Controllers use `HandleResult()` to map to HTTP responses
   - Factory methods in `Result` class:
     - `Result.Success<T>(T data)` - Returns 200 OK
     - `Result.Success<T>(T data, string message)` - Returns 200 OK with message
     - `Result.Error(ErrorResult errorType, string? detail = null)` - Returns error
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
   - Specific repositories available:
     - `_unitOfWork.Products` - IProductRepository (with SearchByNameAsync, ExistsByBarcodeAsync)
     - `_unitOfWork.Customers` - ICustomerRepository (with SearchByNameAsync)
     - `_unitOfWork.Users` - IUserRepository (with SearchByNameAsync, GetByEmailAsync, GetByEmailWithRoleAsync)
     - `_unitOfWork.Sales` - ISaleRepository (with GetByDateRangeAsync, GetByIdWithDetailsAsync, GetAllWithDetailsAsync)
     - `_unitOfWork.Inventories` - IInventoryRepository (with GetByProductIdAsync, GetLowStockItemsAsync)
     - `_unitOfWork.Returns` - IReturnRepository (with GetByStatusAsync, GetBySaleIdAsync)
     - `_unitOfWork.CashRegisters` - ICashRegisterRepository (with GetByIdWithDetailsAsync)
     - `_unitOfWork.Roles` - IRoleRepository
     - `_unitOfWork.RefreshTokens` - IRefreshTokenRepository (with GetActiveTokenAsync, RevokeAllUserTokensAsync)
   - Generic access: `_unitOfWork.Repository<EntityType>()`
   - **Note:** Base repository does NOT support eager loading. Use specific repository methods for eager loading.

5. **Domain Events** - `Domain/Events/`, `Application/DesignPatterns/Events/`
   - `IDomainEvent` / `DomainEvent` base classes
   - `IDomainEventDispatcher` for dispatching events
   - Examples: `SaleCreatedEvent`, `LowStockEvent`, `SaleCancelledEvent`, `ReturnApprovedEvent`
   - Event handlers registered in DI and executed automatically on SaveChangesAsync

6. **Specification Pattern** - `Domain/Specifications/`
   - **Status**: ✅ Fully implemented and actively used
   - Encapsulates query logic (filtering, ordering, paging, eager loading)
   - `ISpecification<T>` / `BaseSpecification<T>` in Domain layer
   - `SpecificationEvaluator<T>` in Infrastructure converts to IQueryable
   - Integrated with `IRepositoryBase<T>` via `ListAsync()` and `CountAsync()`
   - Example specifications in `Domain/Specifications/{Entity}/`

## Database Conventions

- All entities inherit from `BaseEntity` (Guid v7 ID, CreatedAt, UpdatedAt, DeletedAt)
- Catalog entities inherit from `BaseCatalog` (extends BaseEntity with Name, Description?)
  - **Note:** `Description` is nullable (string?) in `BaseCatalog`
- Soft deletes: `DeletedAt` is set instead of removing rows
- Connection string: `SuperPOS` in configuration
- SQL Server via EF Core 10

## Nullable Reference Types

**Status**: ✅ Enabled in all projects (`<Nullable>enable</Nullable>`)

**Important Guidelines**:
- ✅ All nullable warnings have been resolved
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
   - **Validation:** Implement in handlers directly or via FluentValidation

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
  "JwtSettings": {
    "SecretKey": "configure-via-user-secrets-min-32-chars",
    "Issuer": "SuperPOS.API",
    "Audience": "SuperPOS.Client",
    "AccessTokenExpirationMinutes": 30,
    "RefreshTokenExpirationDays": 30,
    "ClockSkewMinutes": 5
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
- `JwtSettings`: Required for JWT authentication. SecretKey must be configured via User Secrets (min 32 characters)
- `EmailSettings`: Required for low stock alert emails (uses MailKit/SMTP)
- `StockSettings.LowStockThreshold`: Triggers email alerts when stock ≤ this value (default: 10)
- `BusinessInfo`: Required for PDF ticket generation (business details on tickets and reports)

### User Secrets
- User secrets ID: `7758d9ef-4f1e-495a-9803-32babd135164`
- Store sensitive data using:
  ```bash
  dotnet user-secrets set "ConnectionStrings:SuperPOS" "your-connection-string"
  dotnet user-secrets set "JwtSettings:SecretKey" "your-secret-key-min-32-chars"
  ```
- **Important:** JWT SecretKey must be at least 32 characters for HS256 algorithm

### Environment Variables
All configuration can be overridden via environment variables using the format:
- `ConnectionStrings__SuperPOS`
- `CorsSettings__Origins__0`

### External NuGet Packages

**Core Packages**:
- Entity Framework Core 10.x
- Mapster / MapsterMapper
- FluentValidation.DependencyInjectionExtensions 12.1.1
- Microsoft.Extensions.*

**Feature-Specific Packages**:
- **BCrypt.Net-Next 4.0.3** - Password hashing
- **MailKit 4.14.1** - SMTP email sending (low stock alerts)
- **MimeKit 4.14.0** - Email message composition (required by MailKit)
- **QuestPDF 2025.12.4** - PDF generation (tickets and reports)
  - Uses Community License (free for non-commercial use)
- **System.IdentityModel.Tokens.Jwt 8.15.0** - JWT token generation and validation
- **Microsoft.AspNetCore.Authentication.JwtBearer 10.0.3** - JWT authentication middleware
- **EnyimMemcachedCore 3.4.5** - Caching (placeholder for future implementation)

## Dependency Injection

Each layer registers its services via extension methods:

### Web.API Layer - `AddWebAPI()`
- Controllers
- OpenAPI documentation
- JSON serialization (with support for Value Object converters)
- `GlobalExceptionHandlingMiddleware` (handles unhandled exceptions, returns ProblemDetails)
- JWT Authentication and Authorization:
  - JWT Bearer authentication scheme
  - Token validation parameters (issuer, audience, signing key, lifetime)
  - Authorization policies: `AdminOnly`, `ManagerOrAbove`, `SellerOrAbove`

### Application Layer - `AddApplication()`
- Custom Mediator with all handlers from assembly
- Mapster configuration (auto-scanned from assembly)
- FluentValidation validators from assembly
- Domain event dispatcher
- Pipeline behaviors (placeholder for cross-cutting concerns)

### Infrastructure Layer - `AddInfrastructure()`
- DbContext (`SuperPOSDbContext`) with SQL Server
- `IUnitOfWork` and all repository implementations
- Domain services implementations:
  - `IProductUniquenessChecker`, `ICustomerUniquenessChecker`, `IUserUniquenessChecker`
  - `ISaleValidationService` (validates customer/user existence)
  - `IStockReservationService` (two-phase stock reservation with commit/rollback)
- Application services:
  - `IEmailService` (MailKit email notifications with HTML templates)
  - `ITicketService` (QuestPDF ticket and report generation)
  - `IJwtTokenService` (JWT token generation, validation, and user ID extraction)
- `IEncryptionService` (password hashing with BCrypt)
- `IDomainEventDispatcher` (dispatches domain events to handlers)
- Event handlers:
  - `LowStockEventHandler` (sends email alerts)
  - `SaleCancelledEventHandler` (restores inventory)
  - `ReturnApprovedEventHandler` (restores inventory)
- Configuration options:
  - `JwtSettings` - JWT configuration
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
   - Logs exception message

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
```

**Option 2: Manual Loading (if specific method doesn't exist)**

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
```

**Best Practice:** Check if a specific repository method exists before implementing manual loading.

### Specification Pattern (Not Currently Used)

**Note:** Specification pattern infrastructure exists but is not currently used. The pattern is available for future implementation if needed.

### Pagination (Not Currently Used)

**Note:** Pagination infrastructure exists (`BasePaginationQuery`, `PaginationDTO<T>`) but is not currently used.

## Services

### IEncryptionService
Located in `Application/Interfaces/Services/`, implemented in Infrastructure.

**Purpose:** Password hashing with BCrypt (VerifyText, EncryptText methods)

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

- **IJwtTokenService** - JWT token management
  - `GenerateAccessToken()` - Generates JWT access token with user claims (userId, email, role, roleId)
  - `GenerateRefreshToken()` - Generates cryptographically secure random refresh token (64 bytes, base64)
  - `ValidateToken()` - Validates JWT token and returns ClaimsPrincipal
  - `GetUserIdFromToken()` - Extracts user ID from token
  - Configuration via `JwtSettings` in appsettings.json
  - Token expiration: Access Token (30 min default), Refresh Token (30 days default)

## Authentication & Authorization

### JWT Authentication System

**Status**: ✅ Fully Implemented

**Architecture**:
- Access Token (JWT): Short-lived token (30 min default) for API authentication
- Refresh Token: Long-lived token (30 days default) for obtaining new access tokens
- Token storage: Refresh tokens stored in database with revocation support
- Password hashing: BCrypt via `IEncryptionService`

**Endpoints**:
- `POST /api/auth/login` - Authenticate with email/password
  - Request: `{ "email": "user@example.com", "password": "..." }`
  - Response: `{ "accessToken": "...", "refreshToken": "...", "accessTokenExpiresAt": "...", "user": {...} }`
  - Account lockout: 5 failed attempts → 30 minutes lockout
- `POST /api/auth/refresh` - Renew access token
  - Request: `{ "refreshToken": "..." }`
  - Response: `{ "accessToken": "...", "accessTokenExpiresAt": "..." }`
- `POST /api/auth/logout` - Revoke refresh token
  - Request: `{ "refreshToken": "..." }`
  - Response: Success message

**JWT Claims**:
- `sub` - User ID (Guid)
- `email` - User email
- `jti` - JWT ID (unique)
- `ClaimTypes.NameIdentifier` - User ID
- `ClaimTypes.Email` - Email
- `ClaimTypes.Role` - Role name (for authorization policies)
- `roleId` - Role ID (Guid)

**Security Features**:
- Account lockout after 5 failed attempts (30 minutes)
- Active/inactive account management (`IsActive` property)
- Login tracking (`LastLoginAt`, `FailedLoginAttempts`)
- Token revocation support
- HMAC-SHA256 signature validation
- Issuer and audience validation
- Clock skew tolerance (5 minutes default)

**User Entity Enhancements**:
```csharp
public bool IsActive { get; set; }              // Active/inactive flag
public DateTime? LastLoginAt { get; set; }      // Last successful login
public int FailedLoginAttempts { get; set; }    // Failed login counter
public DateTime? LockedUntilAt { get; set; }    // Lockout expiration
public bool IsLocked { get; }                   // Computed property

// Domain methods
public void RecordSuccessfulLogin()             // Reset failed attempts, update last login
public void RecordFailedLogin()                 // Increment counter, lock if >= 5 attempts
public void Unlock()                            // Remove lockout
public void Activate() / Deactivate()           // Toggle account status
```

**Configuration** (`appsettings.json`):
```json
{
  "JwtSettings": {
    "SecretKey": "configure-via-user-secrets-min-32-chars",
    "Issuer": "SuperPOS.API",
    "Audience": "SuperPOS.Client",
    "AccessTokenExpirationMinutes": 30,
    "RefreshTokenExpirationDays": 30,
    "ClockSkewMinutes": 5
  }
}
```

**Important**: SecretKey must be configured via User Secrets (min 32 characters for HS256).

### Role-Based Access Control (RBAC)

**Status**: ✅ Fully Implemented

**Available Roles**:
1. **Administrador** (Admin) - Full system access
2. **Gerente** (Manager) - Sales, inventory, reports, user management (read)
3. **Vendedor** (Seller) - Sales and basic queries only

**Authorization Policies**:
- `AdminOnly` - Requires "Administrador" role
- `ManagerOrAbove` - Requires "Administrador" or "Gerente"
- `SellerOrAbove` - Requires "Administrador", "Gerente", or "Vendedor"

**Protected Endpoints**:

All controllers are protected with `[Authorize]` attribute. Specific policies per endpoint:

**AuthController** (`/api/auth`):
- `POST /login` - [AllowAnonymous]
- `POST /refresh` - [AllowAnonymous]
- `POST /logout` - [AllowAnonymous]

**UserController** (`/api/user`):
- `POST` - [AllowAnonymous] (temporary, for creating first admin)
- `GET /{id}` - [Authorize(Policy = "ManagerOrAbove")]
- `GET` (GetAll) - [Authorize(Policy = "ManagerOrAbove")]
- `GET /search` - [Authorize(Policy = "ManagerOrAbove")]
- `PUT /{id}` - [Authorize(Policy = "AdminOnly")]
- `DELETE /{id}` - [Authorize(Policy = "AdminOnly")]

**ProductController** (`/api/product`):
- `POST` - [Authorize(Policy = "ManagerOrAbove")]
- `GET /{id}` - [Authorize(Policy = "SellerOrAbove")]
- `GET` (GetAll) - [Authorize(Policy = "SellerOrAbove")]
- `GET /search/name` - [Authorize(Policy = "SellerOrAbove")]
- `GET /search/barcode` - [Authorize(Policy = "SellerOrAbove")]
- `PUT /{id}` - [Authorize(Policy = "ManagerOrAbove")]
- `DELETE /{id}` - [Authorize(Policy = "AdminOnly")]

**Other Controllers**: See documentation for complete list of protected endpoints on CustomerController, SaleController, InventoryController, CashRegisterController, ReturnController, and RoleController.

**Usage in Client**:
```http
Authorization: Bearer <access-token>
```

**HTTP Response Codes**:
- `401 Unauthorized` - Missing or invalid token
- `403 Forbidden` - Valid token but insufficient permissions
- `200 OK` - Authorized and successful

## Current Project State

### ✅ Fully Implemented Features

1. **Product Management** - CRUD, search by name/barcode, barcode uniqueness
2. **Customer Management** - CRUD, search by name
3. **User Management** - CRUD, search by name with role, password hashing, account lockout, login tracking
4. **Role Management** - CRUD operations
5. **Inventory Management** - Stock adjustment (Add/Set/Remove operations), automatic restoration on sale cancellation/return approval
6. **Sales Management** - Create sales with stock reservation and validation, sale cancellation with inventory rollback, PDF ticket generation
7. **Cash Register** - Create cash register close, automatic sales calculation, PDF report generation
8. **Returns & Exchanges** - Complete return workflow (create, approve, reject), 30-day window validation, automatic inventory restoration
9. **Email Notifications** - Low stock alerts sent to managers (MailKit), email logging (EmailLog entity)
10. **PDF Generation** - Professional tickets and cash register reports (QuestPDF)
11. **Search Functionality** - Products, Customers, and Users
12. **Soft Delete** - All entities support soft delete with timestamp
13. **Value Objects** - Email, PersonName, PhoneNumber, Barcode, Quantity
14. **Domain Events** - Product created/price changed, sale created/cancelled, low stock detected, return approved
15. **Two-Phase Stock Reservation** - Prevents overselling with validate/reserve, commit/rollback pattern
16. **Nullable Reference Types** - All projects have nullable enabled, all warnings resolved
17. **JWT Authentication** - Complete authentication system with access token (30 min) and refresh token (30 days)
18. **Role-Based Access Control (RBAC)** - Complete authorization system with three roles (Admin, Manager, Seller)
19. **Specification Pattern** - Reusable query specifications with filtering, ordering, paging, and eager loading
20. **Password Reset** - Complete password recovery workflow with email verification codes

### ⚠️ Implemented Infrastructure, Not Fully Used

- **Pagination** - `BasePaginationQuery` and `PaginationDTO` exist but only used in ProductGetPagedQuery
- **Specification Pattern** - ✅ Fully implemented and actively used (see section below)

### ❌ Not Implemented

1. **Payment Methods** - Sales do not track payment type (cash, card, etc.)
2. **Product Categories** - No product categorization
3. **Unit Tests** - No test projects exist
4. **Sale Updates** - Sales can be cancelled but not edited (immutable after creation)
5. **Password Recovery** - PasswordResetToken entity exists but workflow not implemented
6. **Advanced Reports** - Sales reports with filters, charts, and export (PDF/Excel)
7. **Dashboard Analytics** - Statistics, trends, and metrics visualization
8. **Chat in Real-Time** - ChatMessage and Conversation entities exist but SignalR not implemented

### Important Notes

- **Validation:** Uses FluentValidation in Application layer. No separate validator files per command/query.
- **Messages:** All user-facing messages are in Spanish
- **Specific Repositories:** Some entities (Sales, Products, etc.) have specialized repository methods. Always check `IUnitOfWork` properties before using generic `Repository<T>()`
- **CreatedAtAction:** All POST endpoints that create resources must pass `nameof(GetById)` to `HandleResult()` for proper Location header generation
- **Domain Events:** Event-driven architecture in place
- **Immutable Sales:** Sales cannot be edited after creation, only cancelled (with automatic inventory rollback)
- **Return Window:** Returns must be requested within 30 days of the original sale
- **Email Configuration:** Requires valid SMTP credentials in EmailSettings. Use Gmail app passwords for development.
- **PDF License:** QuestPDF Community License in use. For commercial production, acquire appropriate license.
- **Nullable References:** All projects have `<Nullable>enable</Nullable>`. Never use `!` operator without null validation.
- **JWT Authentication:** All endpoints (except `/api/auth/*`) require valid JWT Bearer token in Authorization header. SecretKey must be configured via User Secrets (min 32 characters).
- **Authorization Policies:** Three policies available: `AdminOnly`, `ManagerOrAbove`, `SellerOrAbove`
- **Account Security:** Account lockout after 5 failed login attempts for 30 minutes. User accounts can be activated/deactivated via `IsActive` property.
- **Role Names:** Must match exactly in database: "Administrador", "Gerente", "Vendedor" (Spanish names).

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
    [Authorize(Policy = "ManagerOrAbove")]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        var result = await _mediator.Send(command);
        return HandleResult(result, nameof(GetById));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "SellerOrAbove")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new ProductGetByIdQuery(id);
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpGet]
    [Authorize(Policy = "SellerOrAbove")]
    public async Task<IActionResult> GetAll()
    {
        var query = new ProductGetAllQuery();
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }
}
```

### Authentication Handler Example

```csharp
// File: Application/UseCases/Authentication/CQRS/Commands/Login/LoginHandler.cs
using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Application.Interfaces.Services;
using Application.Options;
using Application.UseCases.Authentication.DTOs;
using Domain.Entities.Authentication;
using Domain.Entities.Users;
using Microsoft.Extensions.Options;

namespace Application.UseCases.Authentication.CQRS.Commands.Login;

public class LoginHandler(
    IUnitOfWork unitOfWork,
    IEncryptionService encryptionService,
    IJwtTokenService jwtTokenService,
    IMapper mapper,
    IOptions<JwtSettings> jwtSettings)
    : IRequestHandler<LoginCommand, OperationResult<LoginResponseDTO>>
{
    public async Task<OperationResult<LoginResponseDTO>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get user with role
        var user = await unitOfWork.Users.GetByEmailWithRoleAsync(request.Email, cancellationToken);

        if (user == null)
            return Result.Error(ErrorResult.BadRequest, UserMessages.Authentication.InvalidCredentials);

        // 2. Check if account is locked
        if (user.IsLocked)
        {
            var minutesRemaining = (int)(user.LockedUntilAt!.Value - DateTime.UtcNow).TotalMinutes;
            return Result.Error(ErrorResult.Forbidden,
                string.Format(UserMessages.Authentication.AccountLocked, minutesRemaining));
        }

        // 3. Check if account is active
        if (!user.IsActive)
            return Result.Error(ErrorResult.Forbidden, UserMessages.Authentication.AccountInactive);

        // 4. Validate password
        if (!encryptionService.VerifyText(request.Password, user.PasswordHash))
        {
            user.RecordFailedLogin();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Error(ErrorResult.BadRequest, UserMessages.Authentication.InvalidCredentials);
        }

        // 5. Record successful login
        user.RecordSuccessfulLogin();

        // 6. Generate tokens
        var accessToken = jwtTokenService.GenerateAccessToken(
            user.Id, user.Email.Value, user.RoleId, user.Role.Name);

        var refreshTokenValue = jwtTokenService.GenerateRefreshToken();

        var refreshToken = RefreshToken.Create(
            user.Id,
            refreshTokenValue,
            DateTime.UtcNow.AddDays(jwtSettings.Value.RefreshTokenExpirationDays)
        );

        unitOfWork.RefreshTokens.Add(refreshToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Return response
        var userDto = mapper.Map<UserDTO>(user);

        return Result.Success(new LoginResponseDTO(
            accessToken,
            refreshTokenValue,
            DateTime.UtcNow.AddMinutes(jwtSettings.Value.AccessTokenExpirationMinutes),
            refreshToken.ExpiresAt,
            userDto
        ), UserMessages.Authentication.LoginSuccess);
    }
}
```

### Specification Pattern Example

The Specification pattern encapsulates query logic (filtering, ordering, paging, eager loading) in reusable, testeable classes.

**Architecture**:
- **Domain Layer** - Specifications interfaces and base classes (`Domain/Specifications/`)
- **Infrastructure Layer** - SpecificationEvaluator converts specs to IQueryable (`Infrastructure/Persistence/Specification/`)
- **Application Layer** - Uses specifications in handlers

#### Creating a Specification

```csharp
// File: Domain/Specifications/Products/ProductsByNameSpecification.cs
using Domain.Entities.Products;
using Domain.Specifications;

namespace Domain.Specifications.Products;

/// <summary>
/// Specification to search products by name with optional pagination.
/// </summary>
public class ProductsByNameSpecification : BaseSpecification<Product>
{
    /// <summary>
    /// Search products by name (case-insensitive, partial match).
    /// </summary>
    public ProductsByNameSpecification(string searchTerm)
        : base(p => p.Name.Contains(searchTerm))  // Filter criteria
    {
        // Ordering
        AddOrderBy(p => p.Name);

        // Query optimizations
        SetTracking(false);  // AsNoTracking for read-only
        SetSplitQuery(false); // No split query needed (single table)
    }

    /// <summary>
    /// Search with pagination.
    /// </summary>
    public ProductsByNameSpecification(string searchTerm, int pageIndex, int pageSize)
        : base(p => p.Name.Contains(searchTerm))
    {
        AddOrderBy(p => p.Name);
        ApplyPaging((pageIndex - 1) * pageSize, pageSize);

        SetTracking(false);
        SetSplitQuery(false);
    }
}
```

#### Advanced Specification with Eager Loading

```csharp
// File: Domain/Specifications/Sales/SalesWithDetailsSpecification.cs
using Domain.Entities.Sales;
using Domain.Specifications;

namespace Domain.Specifications.Sales;

public class SalesWithDetailsSpecification : BaseSpecification<Sale>
{
    public SalesWithDetailsSpecification()
    {
        // Eager load related entities
        AddInclude(s => s.Customer);
        AddInclude(s => s.User);
        AddInclude(s => s.SaleDetails);

        // Deep navigation with string-based include
        AddInclude("SaleDetails.Product");

        // Ordering
        AddOrderByDescending(s => s.CreatedAt);

        // Query optimizations
        SetTracking(false);
        SetSplitQuery(true);  // Prevent cartesian explosion with multiple collections
    }

    /// <summary>
    /// Filter by customer with pagination.
    /// </summary>
    public SalesWithDetailsSpecification(Guid customerId, int pageIndex, int pageSize)
        : base(s => s.CustomerId == customerId)
    {
        AddInclude(s => s.Customer);
        AddInclude(s => s.User);
        AddInclude(s => s.SaleDetails);
        AddInclude("SaleDetails.Product");

        AddOrderByDescending(s => s.CreatedAt);
        ApplyPaging((pageIndex - 1) * pageSize, pageSize);

        SetTracking(false);
        SetSplitQuery(true);
    }
}
```

#### Using Specifications in Handlers

```csharp
// File: Application/UseCases/Products/CQRS/Queries/Search/ProductSearchHandler.cs
using Domain.Specifications.Products;

public class ProductSearchHandler : IRequestHandler<ProductSearchQuery, OperationResult<IEnumerable<ProductDTO>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public async Task<OperationResult<IEnumerable<ProductDTO>>> Handle(
        ProductSearchQuery request,
        CancellationToken cancellationToken)
    {
        // Create specification
        var specification = new ProductsByNameSpecification(request.Term);

        // Use ListAsync from repository
        var products = await _unitOfWork.Products.ListAsync(specification, cancellationToken);

        var dtos = _mapper.Map<IEnumerable<ProductDTO>>(products);
        return Result.Success(dtos);
    }
}
```

#### Pagination with Total Count

```csharp
// File: Application/UseCases/Products/CQRS/Queries/GetPaged/ProductGetPagedHandler.cs
using Domain.Specifications.Products;

public class ProductGetPagedHandler : IRequestHandler<ProductGetPagedQuery, OperationResult<PagedProductsDTO>>
{
    public async Task<OperationResult<PagedProductsDTO>> Handle(
        ProductGetPagedQuery request,
        CancellationToken cancellationToken)
    {
        // Create specification with pagination
        var specification = new ProductsByNameSpecification(
            request.SearchTerm,
            request.PageIndex,
            request.PageSize);

        // Get paginated results
        var products = await _unitOfWork.Products.ListAsync(specification, cancellationToken);

        // Get total count (CountAsync ignores paging/ordering, only applies filter)
        var totalCount = await _unitOfWork.Products.CountAsync(specification, cancellationToken);

        // Calculate metadata
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var productDtos = _mapper.Map<List<ProductDTO>>(products);

        return Result.Success(new PagedProductsDTO(
            Items: productDtos,
            TotalCount: totalCount,
            PageIndex: request.PageIndex,
            PageSize: request.PageSize,
            TotalPages: totalPages
        ));
    }
}
```

#### Specification Features

**BaseSpecification<T> provides**:
- `AddOrderBy(expr)` - Primary ascending ordering
- `AddOrderByDescending(expr)` - Primary descending ordering
- `AddThenBy(expr)` - Secondary ascending ordering
- `AddThenByDescending(expr)` - Secondary descending ordering
- `AddInclude(expr)` - Eager load navigation property (lambda)
- `AddInclude(string)` - Eager load deep navigation (e.g., "SaleDetails.Product")
- `ApplyPaging(skip, take)` - Enable pagination
- `SetTracking(bool)` - Enable/disable EF change tracking (default: false)
- `SetSplitQuery(bool)` - Enable/disable split query (default: true)

**Repository Methods**:
- `ListAsync(ISpecification<T> spec)` - Get entities matching specification
- `CountAsync(ISpecification<T> spec)` - Count entities (ignores paging/ordering/includes)

**When to Use Specifications**:
- ✅ Complex queries with multiple criteria
- ✅ Queries with eager loading (Include)
- ✅ Queries with pagination and total count
- ✅ Reusable query logic across multiple handlers
- ✅ Type-safe, testeable query encapsulation

**When NOT to Use**:
- ❌ Simple `GetByIdAsync()` queries
- ❌ Simple `GetAllAsync()` queries without filtering
- ❌ One-off queries not reused elsewhere
