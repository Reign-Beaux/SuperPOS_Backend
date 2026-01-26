# AGENTS.md

## When working with this file:
- Always read the full file before making changes
- Never replace the entire content
- Only append or extend existing sections
- Preserve all existing content verbatim unless explicitly instructed to delete it
- If unsure, do not modify the file and explain the reason

## Role and Behavior
This is a .NET backend solution following Clean Architecture and DDD. The agent must act as a Senior .NET Developer. Use filesystem tools and recursive exploration to interact with the codebase.

### Technical Context & Standards:
- **Generic Repositories**: DO NOT create specific repository interfaces/classes (e.g., `ICustomerRepository`). Use `IUnitOfWork.Repository<T>()` for all standard CRUD operations.
- **Catalogs**: Entities that represent basic lists (Name/Description) must inherit from `BaseCatalog` (resides in `Domain.Entities`).
- **Standard CRUD Location**: `/src/Application/UseCases/[EntityName]/`
  - `CQRS/Commands`: Create, Update, Delete.
  - `CQRS/Queries`: GetAll, GetById, GetPaged.
  - `DTOs`: Data transfer objects.
  - `[Entity]Rules.cs`: Business validation (uniqueness, existence checks).
  - `[Entity]Mappings.cs`: Mapster configuration.

### Rules:
- Use `glob` and `read` tools for file discovery.
- Always verify file existence before modification.
- Follow exact relative paths from the root directory.
- Never assume file contents or purpose beyond confirmed data.
- **Generic-First**: Always check if `IUnitOfWork` satisfies the data access needs. Create specific logic in `Application` layer `Rules` classes, NOT in new repositories.

## Repository Structure
- Solution file: `SuperPOS_Backend.sln` (root directory)
- Projects: `Web.API`, `Application`, `Domain`, `Infrastructure` (all inside `/src/`).

## Project Analysis Protocol
- Always search recursively for `.sln` and `.csproj` files.
- Read the `.sln` file first to understand solution composition.
- Read each `.csproj` file to extract factual information (target framework, project references).
- If relevant files are found, never respond that they were not found.
- If a file cannot be read, explicitly explain the limitation.
- Clearly distinguish between confirmed facts and convention-based interpretation.

## Action-Specific Workflows
The agent MUST follow these precise steps for each requested action:

### 1. Create a New Entity
- **Requirements**: Name, properties, types.
- **Workflow**:
  1. **Domain**: Create class in `src/Domain/Entities/[Folder]/`, inherit from `BaseEntity` (catalogs must inherit from `BaseCatalog`).
  2. **Infrastructure**: Create Configuration in `src/Infrastructure/Persistence/Configurations/`.
  3. **DbContext**: Add `DbSet<Entity>` to both the Interface (e.g., `ISuperPOSDbContext`) and Implementation (`SuperPOSDbContext`). Inside the implementation, place the `DbSet` property before the `OnModelCreating` method.
  4. **Application**: Create directory `src/Application/UseCases/[EntityName]/`.

### 2. Create a Catalog
- **Requirements**: Name, optional extra properties/types.
- **Workflow**:
  1. **Domain**: Create class in `src/Domain/Entities/`, inherit from `BaseCatalog`. Add extra properties if specified.
  2. **Infrastructure**: Create Configuration in `src/Infrastructure/Persistence/Configurations/`.
  3. **DbContext**: Add `DbSet<Entity>` to both Interface and Implementation. Inside the implementation, place the `DbSet` property before the `OnModelCreating` method.
  4. **Application (CRUD)**: Create full CQRS structure in `src/Application/UseCases/[EntityName]/` based on `Roles` pattern:
     - `CQRS/Commands/[Create/Update/Delete]`
     - `CQRS/Queries/[GetAll/GetById]`
     - `DTOs/`, `Rules.cs`, `Mappings.cs`.
  5. **Web.API**: Create `[Entity]Controller.cs` with standard CRUD endpoints.

### 3. Create a Command / Query
- **Requirements**: Entity name, Endpoint name, Parameters (optional).
- **Workflow**:
  1. Create directory in `src/Application/UseCases/[Entity]/CQRS/[Commands|Queries]/[Name]/`.
  2. Create "Empty but Configured" files:
     - **Command/Query**: Class with properties.
     - **Handler**: Class with `Handle` method (returns `OperationResult`).
     - **Validator**: FluentValidation class (for Commands).
  3. Register endpoint in the corresponding Controller.

### 4. Create a Cached Service
- **Workflow**:
  1. **Interface**: Create in `src/Application/Interfaces/Cached/`.
  2. **Implementation**: Create in `src/Infrastructure/Cached/`.
  3. **DI**: Register in `src/Infrastructure/DependencyInjection.cs` inside `AddCaching()`.

### 5. Create an Infrastructure Service
- **Workflow**:
  1. **Interface**: Create in `src/Application/Interfaces/Services/`.
  2. **Implementation**: Create in `src/Infrastructure/Services/`.
  3. **DI**: Register in `src/Infrastructure/DependencyInjection.cs` inside `AddServices()`.

### 6. Create a Middleware
- **Workflow**:
  1. **Web.API**: Create class in `src/Web.API/Middlewares/`.
  2. **DI**: Register in `src/Web.API/DependencyInjection.cs` and configure in `Program.cs`.

### 7. Create a Serialization
- **Workflow**:
  1. **Web.API**: Create in `src/Web.API/Serializations/`.
  2. **DI**: Register in `src/Web.API/DependencyInjection.cs` inside `AddSerializations()`.

### 8. Create a Value Object
- **Workflow**:
  1. **Domain**: Create class in `src/Domain/ValueObjects/`.

### 9. Create a Behavior
- **Workflow**:
  1. **Application**: Create in `src/Application/Behaviors/`.
  2. **DI**: Register in `src/Application/DependencyInjection.cs` inside `AddBehaviors()`.

## Code Change Rules
- **Generic Repositories**: DO NOT create specific repositories. Use `IUnitOfWork.Repository<T>()`.
- **DI Registration**: 
  - New **Services, Behaviors, and Middlewares** MUST be registered in their respective `DependencyInjection.cs`.
  - **MediatR Handlers** do NOT require manual registration as they are discovered automatically via reflection (Standard Mediator pattern).
- **Boilerplate**: When creating Commands/Queries, include the necessary `using` and class structure so they are ready to be implemented.
- **DbContext**: Always check for an interface of the DbContext before modification.
