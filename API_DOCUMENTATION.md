# 📚 SuperPOS Backend - Documentación Completa de API

**Versión:** 1.0
**Última actualización:** 4 de febrero de 2026
**Audiencia:** Equipos de Frontend (React) y IA (Gemini)

---

## 📋 Tabla de Contenidos

1. [Introducción](#introducción)
2. [Configuración Base](#configuración-base)
3. [Resumen de Endpoints](#resumen-de-endpoints)
4. [Tipos de Datos](#tipos-de-datos)
5. [Módulo Customers](#módulo-customers)
6. [Módulo Products](#módulo-products)
7. [Módulo Users](#módulo-users)
8. [Módulo Roles](#módulo-roles)
9. [Módulo Sales](#módulo-sales)
10. [Módulo Inventory](#módulo-inventory)
11. [Códigos HTTP y Errores](#códigos-http-y-errores)
12. [Arquitectura y Patrones](#arquitectura-y-patrones)
13. [Ejemplos de Flujos Comunes](#ejemplos-de-flujos-comunes)

---

## Introducción

**SuperPOS Backend** es una API REST construida con **C# / .NET 10** que implementa un sistema completo de gestión de punto de venta (POS). La API está diseñada siguiendo principios de arquitectura limpia y patrones empresariales como CQRS (Command Query Responsibility Segregation) y Mediator Pattern.

### Características principales:

- ✅ CRUD completo para Clientes, Productos, Usuarios, Roles, Ventas e Inventario
- ✅ Gestión de stock con operaciones (Sumar, Establecer cantidad)
- ✅ Registro detallado de ventas con desglose por producto
- ✅ Soft delete (eliminación lógica) con campo `DeletedAt`
- ✅ Validaciones robustas en cada operación
- ✅ Manejo centralizado de errores
- ✅ CORS habilitado para integración con frontends
- ✅ Generación automática de IDs (GUID v7) y timestamps

---

## Configuración Base

### URL Base

```
http://localhost:5000/api
```

### Headers Requeridos

```json
{
  "Content-Type": "application/json",
  "Accept": "application/json"
}
```

### Autenticación

**Estado actual:** ❌ NO implementada  
**JWT:** Comentado en el código, listo para ser habilitado en futuras versiones

### CORS

- ✅ **Habilitado** - Configurables desde `appsettings.json` → `CorsSettings:Origins`
- Métodos permitidos: `GET`, `POST`, `PUT`, `DELETE`, `OPTIONS`
- Headers soportados: `Content-Type`, `Authorization`

---

## Resumen de Endpoints

| Módulo | Método | Ruta | Descripción |
|--------|--------|------|-------------|
| **Customers** | POST | `/api/customer` | Crear cliente |
| | GET | `/api/customer/{id}` | Obtener cliente por ID |
| | GET | `/api/customer` | Listar todos los clientes |
| | PUT | `/api/customer/{id}` | Actualizar cliente |
| | DELETE | `/api/customer/{id}` | Eliminar cliente (soft delete) |
| **Products** | POST | `/api/product` | Crear producto |
| | GET | `/api/product/{id}` | Obtener producto por ID |
| | GET | `/api/product` | Listar todos los productos |
| | PUT | `/api/product/{id}` | Actualizar producto |
| | DELETE | `/api/product/{id}` | Eliminar producto (soft delete) |
| **Users** | POST | `/api/user` | Crear usuario |
| | GET | `/api/user/{id}` | Obtener usuario por ID |
| | GET | `/api/user` | Listar todos los usuarios |
| | PUT | `/api/user/{id}` | Actualizar usuario |
| | DELETE | `/api/user/{id}` | Eliminar usuario (soft delete) |
| **Roles** | POST | `/api/role` | Crear rol |
| | GET | `/api/role/{id}` | Obtener rol por ID |
| | GET | `/api/role` | Listar todos los roles |
| | PUT | `/api/role/{id}` | Actualizar rol |
| | DELETE | `/api/role/{id}` | Eliminar rol (soft delete) |
| **Sales** | POST | `/api/sale` | Crear venta |
| | GET | `/api/sale/{id}` | Obtener venta por ID |
| | GET | `/api/sale` | Listar todas las ventas |
| **Inventory** | POST | `/api/inventory/adjust` | Ajustar stock de producto |
| | GET | `/api/inventory/product/{productId}` | Obtener inventario de producto |
| | GET | `/api/inventory` | Listar inventario completo |

---

## Tipos de Datos

### Respuesta Genérica: OperationResult

Todas las respuestas siguen este patrón:

```json
{
  "status": 200,
  "value": {
    // Datos de la operación (si es exitosa)
  },
  "error": null
}
```

**En caso de error:**

```json
{
  "status": 400,
  "value": null,
  "error": {
    "title": "Nombre del error",
    "detail": "Descripción detallada del error"
  }
}
```

### Enums

#### StatusResult (Códigos HTTP Mapeados)

```
200 - Ok              (GET exitoso)
201 - Created         (POST exitoso, recurso creado)
204 - NoContent       (PUT/DELETE exitoso, sin contenido)
400 - BadRequest      (Validación fallida)
403 - Forbidden       (Acceso prohibido)
404 - NotFound        (Recurso no encontrado)
409 - Exists          (Conflicto: recurso ya existe)
409 - Conflict        (Conflicto genérico)
500 - InternalServerError
503 - ServiceUnavailable
504 - GatewayTimeout
```

#### InventoryOperation

```
"Add"  → Suma la cantidad al stock actual
"Set"  → Establece la cantidad exacta (reemplaza)
```

### DTOs - Data Transfer Objects

#### CustomerDTO

```json
{
  "id": "uuid (GUID v7)",
  "name": "string",
  "firstLastname": "string",
  "secondLastname": "string | null",
  "phone": "string | null",
  "email": "string | null",
  "birthDate": "ISO 8601 datetime | null"
}
```

#### ProductDTO

```json
{
  "id": "uuid (GUID v7)",
  "name": "string",
  "description": "string | null",
  "barcode": "string | null"
}
```

#### UserDTO

```json
{
  "id": "uuid (GUID v7)",
  "name": "string",
  "firstLastname": "string",
  "secondLastname": "string | null",
  "email": "string",
  "phone": "string | null",
  "roleId": "uuid (GUID v7)",
  "roleName": "string | null"
}
```

**Notas:**
- El campo `password` NUNCA se devuelve en la respuesta.
- Cada usuario tiene asignado un único rol (`roleId`).
- El campo `roleName` es informativo y se carga automáticamente desde la tabla de roles.

#### RoleDTO

```json
{
  "id": "uuid (GUID v7)",
  "name": "string",
  "description": "string | null"
}
```

#### SaleDTO

```json
{
  "id": "uuid (GUID v7)",
  "customerId": "uuid (GUID v7)",
  "customerName": "string",
  "userId": "uuid (GUID v7)",
  "userName": "string",
  "totalAmount": "decimal",
  "createdAt": "ISO 8601 datetime",
  "details": [
    {
      "id": "uuid (GUID v7)",
      "productId": "uuid (GUID v7)",
      "productName": "string",
      "quantity": "integer",
      "unitPrice": "decimal",
      "total": "decimal"
    }
  ]
}
```

#### InventoryDTO

```json
{
  "id": "uuid (GUID v7)",
  "productId": "uuid (GUID v7)",
  "productName": "string",
  "quantity": "integer",
  "createdAt": "ISO 8601 datetime",
  "updatedAt": "ISO 8601 datetime"
}
```

### Entidades Base

#### BaseEntity (Heredada por la mayoría de entidades)

Todos los campos se generan automáticamente:

```json
{
  "id": "uuid (GUID v7) - Generado automáticamente",
  "createdAt": "ISO 8601 datetime - Timestamp de creación",
  "updatedAt": "ISO 8601 datetime | null - Timestamp de última actualización",
  "deletedAt": "ISO 8601 datetime | null - Timestamp de eliminación (soft delete)"
}
```

---

## Módulo Customers

### Crear Cliente

```
POST /api/customer
Content-Type: application/json
```

**Request Body:**

```json
{
  "name": "string (requerido, 1-100 caracteres)",
  "firstLastname": "string (requerido, 1-100 caracteres)",
  "secondLastname": "string (opcional, máx 100 caracteres)",
  "phone": "string (opcional)",
  "email": "string (opcional, debe ser email válido)",
  "birthDate": "ISO 8601 datetime (opcional)"
}
```

**Response Success (201 Created):**

```json
{
  "status": 201,
  "value": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Juan",
    "firstLastname": "Pérez",
    "secondLastname": "García",
    "phone": "+34 123456789",
    "email": "juan.perez@example.com",
    "birthDate": "1990-05-15T00:00:00Z"
  },
  "error": null
}
```

**Response Error (400 Bad Request):**

```json
{
  "status": 400,
  "value": null,
  "error": {
    "title": "Bad Request",
    "detail": "El nombre es requerido"
  }
}
```

### Obtener Cliente por ID

```
GET /api/customer/{id}
```

**Parámetros:**
- `id` (path, uuid): ID del cliente a obtener

**Response Success (200 Ok):**

```json
{
  "status": 200,
  "value": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Juan",
    "firstLastname": "Pérez",
    "secondLastname": "García",
    "phone": "+34 123456789",
    "email": "juan.perez@example.com",
    "birthDate": "1990-05-15T00:00:00Z"
  },
  "error": null
}
```

**Response Not Found (404):**

```json
{
  "status": 404,
  "value": null,
  "error": {
    "title": "Not Found",
    "detail": "Cliente no encontrado"
  }
}
```

### Listar Todos los Clientes

```
GET /api/customer
```

**Response Success (200 Ok):**

```json
{
  "status": 200,
  "value": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "Juan",
      "firstLastname": "Pérez",
      "secondLastname": "García",
      "phone": "+34 123456789",
      "email": "juan.perez@example.com",
      "birthDate": "1990-05-15T00:00:00Z"
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440001",
      "name": "María",
      "firstLastname": "López",
      "secondLastname": "Martínez",
      "phone": "+34 987654321",
      "email": "maria.lopez@example.com",
      "birthDate": "1985-08-22T00:00:00Z"
    }
  ],
  "error": null
}
```

### Actualizar Cliente

```
PUT /api/customer/{id}
Content-Type: application/json
```

**Parámetros:**
- `id` (path, uuid): ID del cliente a actualizar

**Request Body:**

```json
{
  "id": "uuid (requerido, debe coincidir con el id de la ruta)",
  "name": "string (requerido)",
  "firstLastname": "string (requerido)",
  "secondLastname": "string (opcional)",
  "phone": "string (opcional)",
  "email": "string (opcional)",
  "birthDate": "ISO 8601 datetime (opcional)"
}
```

**Response Success (204 No Content):**

```
Respuesta vacía (sin body)
```

**Response Error:**

```json
{
  "status": 400,
  "value": null,
  "error": {
    "title": "Bad Request",
    "detail": "El ID en la ruta no coincide con el ID del body"
  }
}
```

### Eliminar Cliente

```
DELETE /api/customer/{id}
```

**Parámetros:**
- `id` (path, uuid): ID del cliente a eliminar

**Response Success (204 No Content):**

```
Respuesta vacía (sin body)
```

**Nota sobre Soft Delete:** El cliente no se elimina de la base de datos, solo se marca con `deletedAt`. Las consultas posteriores no devolverán clientes eliminados.

---

## Módulo Products

### Crear Producto

```
POST /api/product
Content-Type: application/json
```

**Request Body:**

```json
{
  "name": "string (requerido, 1-100 caracteres)",
  "description": "string (opcional)",
  "barcode": "string (opcional)"
}
```

**Response Success (201 Created):**

```json
{
  "status": 201,
  "value": {
    "id": "550e8400-e29b-41d4-a716-446655440002",
    "name": "Laptop Dell",
    "description": "Laptop Dell XPS 13, 16GB RAM, SSD 512GB",
    "barcode": "8471234567890"
  },
  "error": null
}
```

### Obtener Producto por ID

```
GET /api/product/{id}
```

**Response Success (200 Ok):**

```json
{
  "status": 200,
  "value": {
    "id": "550e8400-e29b-41d4-a716-446655440002",
    "name": "Laptop Dell",
    "description": "Laptop Dell XPS 13, 16GB RAM, SSD 512GB",
    "barcode": "8471234567890"
  },
  "error": null
}
```

### Listar Todos los Productos

```
GET /api/product
```

**Response Success (200 Ok):**

```json
{
  "status": 200,
  "value": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440002",
      "name": "Laptop Dell",
      "description": "Laptop Dell XPS 13, 16GB RAM, SSD 512GB",
      "barcode": "8471234567890"
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440003",
      "name": "Mouse Inalámbrico",
      "description": "Mouse Logitech M705",
      "barcode": "5099206007805"
    }
  ],
  "error": null
}
```

### Actualizar Producto

```
PUT /api/product/{id}
Content-Type: application/json
```

**Request Body:**

```json
{
  "id": "uuid (requerido)",
  "name": "string (requerido)",
  "description": "string (opcional)",
  "barcode": "string (opcional)"
}
```

**Response Success (204 No Content):**

```
Respuesta vacía
```

### Eliminar Producto

```
DELETE /api/product/{id}
```

**Response Success (204 No Content):**

```
Respuesta vacía
```

---

## Módulo Users

### Crear Usuario

```
POST /api/user
Content-Type: application/json
```

**Request Body:**

```json
{
  "name": "string (requerido, 1-100 caracteres)",
  "firstLastname": "string (requerido, 1-100 caracteres)",
  "secondLastname": "string (opcional, máx 100 caracteres)",
  "email": "string (requerido, email válido)",
  "password": "string (requerido, mínimo 8 caracteres)",
  "phone": "string (opcional)",
  "roleId": "uuid (requerido - ID del rol a asignar)"
}
```

**Ejemplo:**

```json
{
  "name": "Carlos",
  "firstLastname": "Rodríguez",
  "secondLastname": "Martínez",
  "email": "carlos.rodriguez@example.com",
  "password": "SecurePassword123!",
  "phone": "+34 555123456",
  "roleId": "550e8400-e29b-41d4-a716-446655440005"
}
```

**Response Success (201 Created):**

```json
{
  "status": 201,
  "value": {
    "id": "550e8400-e29b-41d4-a716-446655440004",
    "name": "Carlos",
    "firstLastname": "Rodríguez",
    "secondLastname": "Martínez",
    "email": "carlos.rodriguez@example.com",
    "phone": "+34 555123456",
    "roleId": "550e8400-e29b-41d4-a716-446655440005",
    "roleName": "Gerente"
  },
  "error": null
}
```

**Response Error - Rol no encontrado (404):**

```json
{
  "status": 404,
  "value": null,
  "error": {
    "title": "Not Found",
    "detail": "Role with ID 550e8400-e29b-41d4-a716-446655440005 not found"
  }
}
```

**Notas:**
- El campo `password` se hashea en la base de datos y NUNCA se devuelve.
- El `roleId` debe existir en la tabla de roles antes de crear el usuario.
- Cada usuario tiene asignado exactamente un rol.

### Obtener Usuario por ID

```
GET /api/user/{id}
```

**Response Success (200 Ok):**

```json
{
  "status": 200,
  "value": {
    "id": "550e8400-e29b-41d4-a716-446655440004",
    "name": "Carlos",
    "firstLastname": "Rodríguez",
    "secondLastname": "Martínez",
    "email": "carlos.rodriguez@example.com",
    "phone": "+34 555123456",
    "roleId": "550e8400-e29b-41d4-a716-446655440005",
    "roleName": "Gerente"
  },
  "error": null
}
```

### Listar Todos los Usuarios

```
GET /api/user
```

**Response Success (200 Ok):**

```json
{
  "status": 200,
  "value": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440004",
      "name": "Carlos",
      "firstLastname": "Rodríguez",
      "secondLastname": "Martínez",
      "email": "carlos.rodriguez@example.com",
      "phone": "+34 555123456",
      "roleId": "550e8400-e29b-41d4-a716-446655440005",
      "roleName": "Gerente"
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440007",
      "name": "Ana",
      "firstLastname": "López",
      "secondLastname": "García",
      "email": "ana.lopez@example.com",
      "phone": "+34 666777888",
      "roleId": "550e8400-e29b-41d4-a716-446655440006",
      "roleName": "Vendedor"
    }
  ],
  "error": null
}
```

### Actualizar Usuario

```
PUT /api/user/{id}
Content-Type: application/json
```

**Request Body:**

```json
{
  "id": "uuid (requerido)",
  "name": "string (requerido)",
  "firstLastname": "string (requerido)",
  "secondLastname": "string (opcional)",
  "email": "string (requerido)",
  "password": "string (opcional - si se envía, se actualiza)",
  "phone": "string (opcional)",
  "roleId": "uuid (requerido - ID del rol a asignar)"
}
```

**Ejemplo:**

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440004",
  "name": "Carlos",
  "firstLastname": "Rodríguez",
  "secondLastname": "Martínez",
  "email": "carlos.nuevo@example.com",
  "password": null,
  "phone": "+34 555999888",
  "roleId": "550e8400-e29b-41d4-a716-446655440006"
}
```

**Response Success (204 No Content):**

```
Respuesta vacía
```

**Response Error - Rol no encontrado (404):**

```json
{
  "status": 404,
  "value": null,
  "error": {
    "title": "Not Found",
    "detail": "Role with ID 550e8400-e29b-41d4-a716-446655440006 not found"
  }
}
```

**Notas:**
- Si `password` es `null` u omitido, la contraseña no se modifica.
- El `roleId` debe existir en la tabla de roles.
- Si se cambia el `roleId`, el usuario se asigna inmediatamente al nuevo rol.

### Eliminar Usuario

```
DELETE /api/user/{id}
```

**Response Success (204 No Content):**

```
Respuesta vacía
```

---

## Módulo Roles

### Crear Rol

```
POST /api/role
Content-Type: application/json
```

**Request Body:**

```json
{
  "name": "string (requerido, 1-100 caracteres)",
  "description": "string (opcional)"
}
```

**Response Success (201 Created):**

```json
{
  "status": 201,
  "value": {
    "id": "550e8400-e29b-41d4-a716-446655440005",
    "name": "Gerente",
    "description": "Rol con permisos de administración completa"
  },
  "error": null
}
```

### Obtener Rol por ID

```
GET /api/role/{id}
```

**Response Success (200 Ok):**

```json
{
  "status": 200,
  "value": {
    "id": "550e8400-e29b-41d4-a716-446655440005",
    "name": "Gerente",
    "description": "Rol con permisos de administración completa"
  },
  "error": null
}
```

### Listar Todos los Roles

```
GET /api/role
```

**Response Success (200 Ok):**

```json
{
  "status": 200,
  "value": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440005",
      "name": "Gerente",
      "description": "Rol con permisos de administración completa"
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440006",
      "name": "Vendedor",
      "description": "Rol básico para vendedores"
    }
  ],
  "error": null
}
```

### Actualizar Rol

```
PUT /api/role/{id}
Content-Type: application/json
```

**Request Body:**

```json
{
  "id": "uuid (requerido)",
  "name": "string (requerido)",
  "description": "string (opcional)"
}
```

**Response Success (204 No Content):**

```
Respuesta vacía
```

### Eliminar Rol

```
DELETE /api/role/{id}
```

**Response Success (204 No Content):**

```
Respuesta vacía
```

---

## Módulo Sales

### Crear Venta

```
POST /api/sale
Content-Type: application/json
```

**Request Body:**

```json
{
  "customerId": "uuid (requerido - ID del cliente)",
  "userId": "uuid (requerido - ID del usuario que realiza la venta)",
  "items": [
    {
      "productId": "uuid (requerido)",
      "quantity": "integer (requerido, > 0)"
    },
    {
      "productId": "uuid (requerido)",
      "quantity": "integer (requerido, > 0)"
    }
  ]
}
```

**Ejemplo:**

```json
{
  "customerId": "550e8400-e29b-41d4-a716-446655440000",
  "userId": "550e8400-e29b-41d4-a716-446655440004",
  "items": [
    {
      "productId": "550e8400-e29b-41d4-a716-446655440002",
      "quantity": 2
    },
    {
      "productId": "550e8400-e29b-41d4-a716-446655440003",
      "quantity": 1
    }
  ]
}
```

**Response Success (201 Created):**

```json
{
  "status": 201,
  "value": {
    "id": "550e8400-e29b-41d4-a716-446655440100",
    "customerId": "550e8400-e29b-41d4-a716-446655440000",
    "customerName": "Juan Pérez García",
    "userId": "550e8400-e29b-41d4-a716-446655440004",
    "userName": "Carlos Rodríguez Martínez",
    "totalAmount": 1500.00,
    "createdAt": "2026-02-03T14:30:00Z",
    "details": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440101",
        "productId": "550e8400-e29b-41d4-a716-446655440002",
        "productName": "Laptop Dell",
        "quantity": 2,
        "unitPrice": 600.00,
        "total": 1200.00
      },
      {
        "id": "550e8400-e29b-41d4-a716-446655440102",
        "productId": "550e8400-e29b-41d4-a716-446655440003",
        "productName": "Mouse Inalámbrico",
        "quantity": 1,
        "unitPrice": 300.00,
        "total": 300.00
      }
    ]
  },
  "error": null
}
```

**Response Error - Stock insuficiente (409 Conflict):**

```json
{
  "status": 409,
  "value": null,
  "error": {
    "title": "Conflict",
    "detail": "Stock insuficiente para el producto: Laptop Dell. Stock disponible: 1, solicitado: 2"
  }
}
```

**Response Error - Cliente no encontrado (404):**

```json
{
  "status": 404,
  "value": null,
  "error": {
    "title": "Not Found",
    "detail": "Cliente no encontrado"
  }
}
```

### Obtener Venta por ID

```
GET /api/sale/{id}
```

**Response Success (200 Ok):**

```json
{
  "status": 200,
  "value": {
    "id": "550e8400-e29b-41d4-a716-446655440100",
    "customerId": "550e8400-e29b-41d4-a716-446655440000",
    "customerName": "Juan Pérez García",
    "userId": "550e8400-e29b-41d4-a716-446655440004",
    "userName": "Carlos Rodríguez Martínez",
    "totalAmount": 1500.00,
    "createdAt": "2026-02-03T14:30:00Z",
    "details": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440101",
        "productId": "550e8400-e29b-41d4-a716-446655440002",
        "productName": "Laptop Dell",
        "quantity": 2,
        "unitPrice": 600.00,
        "total": 1200.00
      },
      {
        "id": "550e8400-e29b-41d4-a716-446655440102",
        "productId": "550e8400-e29b-41d4-a716-446655440003",
        "productName": "Mouse Inalámbrico",
        "quantity": 1,
        "unitPrice": 300.00,
        "total": 300.00
      }
    ]
  },
  "error": null
}
```

### Listar Todas las Ventas

```
GET /api/sale
```

**Response Success (200 Ok):**

```json
{
  "status": 200,
  "value": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440100",
      "customerId": "550e8400-e29b-41d4-a716-446655440000",
      "customerName": "Juan Pérez García",
      "userId": "550e8400-e29b-41d4-a716-446655440004",
      "userName": "Carlos Rodríguez Martínez",
      "totalAmount": 1500.00,
      "createdAt": "2026-02-03T14:30:00Z",
      "details": [...]
    }
  ],
  "error": null
}
```

---

## Módulo Inventory

### Ajustar Stock de Producto

```
POST /api/inventory/adjust
Content-Type: application/json
```

**Request Body:**

```json
{
  "productId": "uuid (requerido)",
  "quantity": "integer (requerido, puede ser positivo o negativo)",
  "operation": "enum (requerido) - 'Add' o 'Set'"
}
```

**Valores de operation:**
- `Add`: Suma la cantidad al stock actual
- `Set`: Establece la cantidad exacta (reemplaza el stock actual)

**Ejemplo - Agregar stock:**

```json
{
  "productId": "550e8400-e29b-41d4-a716-446655440002",
  "quantity": 10,
  "operation": "Add"
}
```

**Ejemplo - Establecer stock exacto:**

```json
{
  "productId": "550e8400-e29b-41d4-a716-446655440002",
  "quantity": 50,
  "operation": "Set"
}
```

**Response Success (201 Created):**

```json
{
  "status": 201,
  "value": {
    "id": "550e8400-e29b-41d4-a716-446655440200",
    "productId": "550e8400-e29b-41d4-a716-446655440002",
    "productName": "Laptop Dell",
    "quantity": 50,
    "createdAt": "2026-02-03T15:00:00Z",
    "updatedAt": "2026-02-03T15:00:00Z"
  },
  "error": null
}
```

**Response Error - Producto no encontrado (404):**

```json
{
  "status": 404,
  "value": null,
  "error": {
    "title": "Not Found",
    "detail": "Producto no encontrado"
  }
}
```

**Response Error - Cantidad inválida (400):**

```json
{
  "status": 400,
  "value": null,
  "error": {
    "title": "Bad Request",
    "detail": "La cantidad no puede ser negativa"
  }
}
```

### Obtener Inventario de Producto

```
GET /api/inventory/product/{productId}
```

**Parámetros:**
- `productId` (path, uuid): ID del producto

**Response Success (200 Ok):**

```json
{
  "status": 200,
  "value": {
    "id": "550e8400-e29b-41d4-a716-446655440200",
    "productId": "550e8400-e29b-41d4-a716-446655440002",
    "productName": "Laptop Dell",
    "quantity": 50,
    "createdAt": "2026-02-03T15:00:00Z",
    "updatedAt": "2026-02-03T15:00:00Z"
  },
  "error": null
}
```

### Listar Inventario Completo

```
GET /api/inventory
```

**Response Success (200 Ok):**

```json
{
  "status": 200,
  "value": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440200",
      "productId": "550e8400-e29b-41d4-a716-446655440002",
      "productName": "Laptop Dell",
      "quantity": 50,
      "createdAt": "2026-02-03T15:00:00Z",
      "updatedAt": "2026-02-03T15:00:00Z"
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440201",
      "productId": "550e8400-e29b-41d4-a716-446655440003",
      "productName": "Mouse Inalámbrico",
      "quantity": 150,
      "createdAt": "2026-02-03T15:05:00Z",
      "updatedAt": "2026-02-03T15:05:00Z"
    }
  ],
  "error": null
}
```

---

## Códigos HTTP y Errores

### Tabla de Códigos HTTP

| Código | Nombre | Significado | Cuándo Ocurre |
|--------|--------|-------------|---------------|
| **200** | Ok | Éxito - GET completado | Cuando se obtiene un recurso exitosamente |
| **201** | Created | Éxito - Recurso creado | Cuando se crea un nuevo recurso (POST) |
| **204** | No Content | Éxito - Sin contenido | PUT/DELETE exitosos, la respuesta es vacía |
| **400** | Bad Request | Error de validación | Campos faltantes, formatos inválidos, etc. |
| **403** | Forbidden | Acceso prohibido | El usuario no tiene permisos |
| **404** | Not Found | Recurso no encontrado | El ID proporcionado no existe |
| **409** | Conflict | Conflicto | Recurso ya existe, stock insuficiente, etc. |
| **500** | Internal Server Error | Error del servidor | Excepción no manejada en el backend |
| **503** | Service Unavailable | Servicio no disponible | Base de datos caída, etc. |
| **504** | Gateway Timeout | Timeout | La operación tardó demasiado |

### Estructura de Error Genérica

Todos los errores devuelven este formato:

```json
{
  "status": 400,
  "value": null,
  "error": {
    "title": "Bad Request",
    "detail": "Descripción específica del error ocurrido"
  }
}
```

### Errores Comunes por Módulo

#### Customers / Products / Users / Roles

| Error | Código | Causa |
|-------|--------|-------|
| `Not Found` | 404 | El ID proporcionado no existe |
| `Bad Request - Campo requerido` | 400 | Falta un campo obligatorio |
| `Bad Request - Formato inválido` | 400 | Email inválido, UUID inválido, etc. |
| `Conflict` | 409 | El recurso ya existe (ej: email duplicado) |

#### Sales

| Error | Código | Causa |
|-------|--------|-------|
| `Not Found - Cliente` | 404 | El cliente no existe |
| `Not Found - Producto` | 404 | Un producto no existe |
| `Not Found - Usuario` | 404 | El usuario no existe |
| `Conflict - Stock insuficiente` | 409 | No hay suficiente stock del producto |
| `Bad Request - Cantidad inválida` | 400 | La cantidad es <= 0 |

#### Inventory

| Error | Código | Causa |
|-------|--------|-------|
| `Not Found - Producto` | 404 | El producto no existe |
| `Bad Request - Cantidad negativa` | 400 | Se intenta ajustar a cantidad negativa |
| `Bad Request - Operation inválida` | 400 | Operation no es "Add" o "Set" |

---

## Arquitectura y Patrones

### Patrón CQRS (Command Query Responsibility Segregation)

La API separa las operaciones de lectura (Queries) de las de escritura (Commands):

- **Commands:** `CreateCustomerCommand`, `UpdateProductCommand`, `DeleteUserCommand`, etc.
  - Modifican estado
  - Retornan 201 (Created) o 204 (No Content)
  
- **Queries:** `GetCustomerByIdQuery`, `GetAllProductsQuery`, etc.
  - Solo leen datos
  - Retornan 200 (Ok) con datos

### Patrón Mediator

Los controladores delegan la ejecución de Commands/Queries a un **Mediator**:

```csharp
// En el controlador:
var result = await _mediator.Send(new CreateCustomerCommand(...));
```

El Mediator:
1. Recibe el comando/query
2. Encuentra el handler correspondiente
3. Ejecuta validaciones
4. Ejecuta el comando/query
5. Devuelve el resultado

### Patrón OperationResult

Todas las operaciones devuelven un `OperationResult<T>` que encapsula:

```csharp
public class OperationResult<T>
{
    public StatusResult Status { get; }      // Código HTTP
    public T? Value { get; }                  // Datos (si éxito)
    public Error? Error { get; }              // Error (si falla)
    public bool IsSuccess { get; }            // True si Status es Ok, Created, NoContent
}
```

### Validaciones

Cada comando/query tiene un validador using **FluentValidation**:

- Valida antes de ejecutar la operación
- Retorna 400 Bad Request con descripción del error si falla
- Ejemplos de validaciones:
  - Campos requeridos no vacíos
  - Longitud máxima/mínima
  - Formato correcto (email, UUID, etc.)
  - Lógica de negocio (stock disponible, etc.)

### Soft Delete

Las entidades eliminadas no se borran de la base de datos:

```json
{
  "id": "...",
  "name": "...",
  "deletedAt": "2026-02-03T10:30:00Z"  // Marcada como eliminada
}
```

**Comportamiento:**
- Al eliminar, se establece el campo `deletedAt`
- Las queries futuras no devuelven registros eliminados
- Los datos pueden recuperarse con acceso directo a la BD si es necesario

### Timestamps Automáticos

Cada entidad tiene:

- `createdAt`: Se establece al crear, nunca cambia
- `updatedAt`: Se actualiza cada vez que se modifica
- `deletedAt`: Se establece solo al eliminar (null si no está eliminada)

---

## Ejemplos de Flujos Comunes

### Flujo 1: Crear una Venta Completa

**Paso 1: Crear cliente**

```json
POST /api/customer
Content-Type: application/json

{
  "name": "Juan",
  "firstLastname": "Pérez",
  "secondLastname": "García",
  "phone": "+34 123456789",
  "email": "juan.perez@example.com"
}

Response (201):
{
  "status": 201,
  "value": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Juan",
    ...
  }
}
```

**Paso 2: Crear usuario (vendedor)**

```json
POST /api/user
Content-Type: application/json

{
  "name": "Carlos",
  "firstLastname": "Rodríguez",
  "secondLastname": "Martínez",
  "email": "carlos@example.com",
  "password": "SecurePassword123!",
  "roleId": "550e8400-e29b-41d4-a716-446655440006"
}

Response (201):
{
  "status": 201,
  "value": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "roleId": "550e8400-e29b-41d4-a716-446655440006",
    "roleName": "Vendedor",
    ...
  }
}
```

**Paso 3: Crear productos**

```json
POST /api/product
Content-Type: application/json

{
  "name": "Laptop Dell",
  "description": "XPS 13, 16GB RAM",
  "barcode": "8471234567890"
}

Response (201):
{
  "status": 201,
  "value": {
    "id": "550e8400-e29b-41d4-a716-446655440002",
    ...
  }
}
```

**Paso 4: Ajustar inventario**

```json
POST /api/inventory/adjust
Content-Type: application/json

{
  "productId": "550e8400-e29b-41d4-a716-446655440002",
  "quantity": 100,
  "operation": "Set"
}

Response (201):
{
  "status": 201,
  "value": {
    "id": "550e8400-e29b-41d4-a716-446655440100",
    "quantity": 100,
    ...
  }
}
```

**Paso 5: Realizar venta**

```json
POST /api/sale
Content-Type: application/json

{
  "customerId": "550e8400-e29b-41d4-a716-446655440000",
  "userId": "550e8400-e29b-41d4-a716-446655440001",
  "items": [
    {
      "productId": "550e8400-e29b-41d4-a716-446655440002",
      "quantity": 2
    }
  ]
}

Response (201):
{
  "status": 201,
  "value": {
    "id": "550e8400-e29b-41d4-a716-446655440200",
    "customerId": "550e8400-e29b-41d4-a716-446655440000",
    "customerName": "Juan Pérez García",
    "userId": "550e8400-e29b-41d4-a716-446655440001",
    "userName": "Carlos Rodríguez Martínez",
    "totalAmount": 1200.00,
    "details": [
      {
        "productId": "550e8400-e29b-41d4-a716-446655440002",
        "productName": "Laptop Dell",
        "quantity": 2,
        "unitPrice": 600.00,
        "total": 1200.00
      }
    ]
  }
}
```

**Resultado:** El stock se reduce automáticamente de 100 a 98.

### Flujo 2: Manejo de Error - Stock Insuficiente

```json
POST /api/sale
Content-Type: application/json

{
  "customerId": "550e8400-e29b-41d4-a716-446655440000",
  "userId": "550e8400-e29b-41d4-a716-446655440001",
  "items": [
    {
      "productId": "550e8400-e29b-41d4-a716-446655440002",
      "quantity": 200  // Hay solo 98 en stock
    }
  ]
}

Response (409 Conflict):
{
  "status": 409,
  "value": null,
  "error": {
    "title": "Conflict",
    "detail": "Stock insuficiente para el producto: Laptop Dell. Stock disponible: 98, solicitado: 200"
  }
}
```

### Flujo 3: Actualizar Cliente

```json
PUT /api/customer/550e8400-e29b-41d4-a716-446655440000
Content-Type: application/json

{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Juan",
  "firstLastname": "Pérez",
  "secondLastname": "García",
  "phone": "+34 999888777",  // Actualizado
  "email": "juan.nuevo@example.com"  // Actualizado
}

Response (204 No Content):
(sin body)
```

**El campo `updatedAt` se establece automáticamente en la BD.**

### Flujo 4: Listar y Filtrar

```
GET /api/product
Content-Type: application/json

Response (200):
{
  "status": 200,
  "value": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440002",
      "name": "Laptop Dell",
      ...
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440003",
      "name": "Mouse Inalámbrico",
      ...
    }
  ]
}
```

---

## Notas Importantes para Integración

### 1. IDs (GUIDs)

- Todos los IDs son **GUID v7** (generados automáticamente por el servidor)
- Formato: `550e8400-e29b-41d4-a716-446655440000`
- En requests de actualización/eliminación, el ID en la ruta debe coincidir con el ID en el body (si aplica)

### 2. Fechas y Horas

- Formato: **ISO 8601** con zona horaria Z (UTC)
- Ejemplo: `2026-02-03T14:30:00Z`
- El servidor establece automáticamente `createdAt`, `updatedAt`, `deletedAt`

### 3. Campos Opcionales

- Los campos opcionales pueden omitirse del request
- Si se envían como `null`, se guardan como `null` en la BD
- Si no se envían, se ignoran

### 4. Validaciones Automáticas

- Email debe ser válido (formato correcto)
- UUIDs deben estar en formato correcto
- Strings tienen límites de longitud
- Las cantidades deben ser > 0

### 5. Soft Delete

- No hay endpoint para "restaurar" un recurso eliminado
- Los GET no devuelven recursos con `deletedAt != null`
- Para acceder a datos eliminados, se requiere acceso directo a BD

### 6. Casos de Uso Especiales

- **Crear venta:** Reduce automáticamente el stock
- **Ajustar inventario con "Add":** Suma a cantidad actual
- **Ajustar inventario con "Set":** Reemplaza cantidad actual
- **Eliminar usuario:** Se marca como `deletedAt` (no se pueden usar en nuevas ventas)

### 7. Headers Necesarios

```
Content-Type: application/json
Accept: application/json
```

**Nota:** Si la autenticación JWT se implementa en futuras versiones, se agregará:
```
Authorization: Bearer <token>
```

---

## Cambios Futuros Planeados

- ✅ Implementación de autenticación JWT
- ✅ Autorización basada en roles
- ✅ Endpoints de reporte (ventas por período, productos más vendidos, etc.)
- ✅ Paginación en listar endpoints
- ✅ Búsqueda y filtros avanzados
- ✅ Webhooks para eventos (venta creada, stock bajo, etc.)
- ✅ Importación/exportación de datos (CSV, Excel)

---

**Última actualización:** 4 de febrero de 2026
**Versión API:** 1.0
**Estado:** Producción Beta
