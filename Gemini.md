# SuperPOS Backend - Gemini Context

Este documento sirve como punto de entrada y contexto técnico para el asistente de IA (Gemini) y los desarrolladores que trabajan en el proyecto **SuperPOS_Backend**.

## 1. Visión General
**SuperPOS_Backend** es una API RESTful construida con **.NET 10** siguiendo los principios de **Clean Architecture**. Su propósito es gestionar la lógica de backend para un sistema de Punto de Venta (POS).

## 2. Tech Stack Principal
- **Framework**: .NET 10.0
- **Lenguaje**: C# 13 (implícito en .NET 10)
- **Base de Datos**: SQL Server
- **ORM**: Entity Framework Core 10.0
- **Validación**: FluentValidation
- **Mapeo**: Mapster
- **Autenticación**: JWT (System.IdentityModel.Tokens.Jwt)
- **Seguridad**: BCrypt.Net-Next (Hashing de contraseñas)
- **Caching**: EnyimMemcachedCore (Memcached)
- **Documentación API**: OpenAPI / Swagger (Nativo de .NET 10)

## 3. Arquitectura (Clean Architecture)
El proyecto está dividido en cuatro capas principales dentro del directorio `src`:

### 3.1. Domain (`src/Domain`)
Núcleo del negocio. No tiene dependencias de otros proyectos.
- **Contenido**: Entidades, Value Objects, Excepciones de Dominio, Interfaces de repositorio (abstracciones).
- **Reglas**: Puro C#, agnóstico de la infraestructura.

### 3.2. Application (`src/Application`)
Contiene la lógica de la aplicación y coordinada los casos de uso.
- **Dependencias**: `Domain`.
- **Contenido**:
    - **CQRS**: Comandos (Commands) y Consultas (Queries).
    - **DTOs**: Objetos de Transferencia de Datos.
    - **Validators**: Reglas de validación con FluentValidation.
    - **Mappings**: Configuraciones de Mapster.
    - **Behaviors**: Comportamientos de pipeline (si se usa MediatR o similar).

### 3.3. Infrastructure (`src/Infrastructure`)
Implementación concreta de las abstracciones definidas en Application/Domain.
- **Dependencias**: `Application`.
- **Contenido**:
    - **Persistence**: `SuperPOSDbContext`, Migraciones de EF Core.
    - **Services**: Implementaciones de servicios externos (Email, Archivos, Auth).
    - **Auth**: Implementación de generación de Tokens JWT.

### 3.4. Web.API (`src/Web.API`)
Punto de entrada de la aplicación.
- **Dependencias**: `Application`, `Infrastructure`.
- **Contenido**:
    - **Controllers**: Endpoints de la API.
    - **Program.cs**: Configuración del contenedor DI, Middleware, CORS.
    - **Middlewares**: Manejo global de excepciones (`GlobalExceptionHandlingMiddleware`).

## 4. Estructura de Directorios Clave
```
/src
  /Domain
    /Entities           # Modelos de dominio
    /ValueObjects       # Objetos de valor
    /Exceptions         # Excepciones personalizadas
  /Application
    /UseCases           # Vertical Slices / Features (por entidad)
      /Articles
        /CQRS           # Commands y Queries
        /DTOs           # Data Transfer Objects
    /DesignPatterns     # Implementaciones de patrones
  /Infrastructure
    /Persistence        # DbContext y configuraciones de EF
    /Identity           # Lógica de Auth
  /Web.API
    /Controllers        # Controladores API
    /Middlewares        # Middlewares HTTP
```

## 5. Convenciones y Patrones
- **CQRS**: Se utiliza el patrón Command/Query para separar operaciones de escritura y lectura.
- **Result Pattern**: Las operaciones de Application suelen retornar un objeto `Result<T>` o similar para manejar errores de dominio sin lanzar excepciones.
- **Global Exception Handling**: Middleware centralizado para capturar errores no controlados y devolver respuestas estandarizadas (ProblemDetails).
- **Environment Variables**: La configuración sensible se maneja via User Secrets en desarrollo y Variables de Entorno en producción.
- **Migrations**: Las migraciones se aplican automáticamente en desarrollo (`app.ApplyMigrations()`).

## 6. Comandos Útiles

### Ejecutar la aplicación
```bash
dotnet run --project src/Web.API/Web.API.csproj
```

### Crear una nueva migración
```bash
dotnet ef migrations add <NombreMigracion> -p src/Infrastructure -s src/Web.API
```

### Actualizar base de datos
```bash
dotnet ef database update -p src/Infrastructure -s src/Web.API
```

## 7. Flujo de Desarrollo (Ejemplo: Nueva Feature)
1.  **Domain**: Definir Entidad o reglas de negocio necesarias.
2.  **Application**:
    -   Crear el DTO de entrada y salida.
    -   Crear el Command/Query.
    -   Implementar el Handler.
    -   Agregar Validaciones.
3.  **Infrastructure**: Si se requiere, agregar configuraciones de EF Core o nuevos servicios.
4.  **Web.API**: Crear o actualizar el Controller para exponer el endpoint.

---
*Este documento debe actualizarse conforme evoluciona la arquitectura y el stack tecnológico del proyecto.*
