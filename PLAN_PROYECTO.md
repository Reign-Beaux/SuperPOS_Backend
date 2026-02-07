# 📋 Plan de Trabajo - SuperPOS Backend

## 🎯 Objetivo del Proyecto
Desarrollar un sistema completo de Punto de Venta (POS) con funcionalidades avanzadas para aprender tecnologías modernas y mejores prácticas de desarrollo.

---

## 📊 Estado Actual del Proyecto

### ✅ Completado
- ✓ Clean Architecture implementada (4 capas: Domain, Application, Infrastructure, Web.API)
- ✓ Domain-Driven Design (DDD) con entidades y value objects
- ✓ CQRS con mediador personalizado
- ✓ Repository Pattern + Unit of Work
- ✓ Entidades principales: User, Role, Customer, Product, Inventory, Sale, SaleDetail
- ✓ CRUDs básicos para la mayoría de entidades
- ✓ Mapster para mapeo de objetos
- ✓ FluentValidation para validaciones
- ✓ EF Core 10 con SQL Server
- ✓ Soft deletes implementados

### 🔄 En Progreso / Parcial
- ⚠ Lógica de negocio de ventas (falta descuento automático de inventario)
- ⚠ Validaciones de negocio complejas

### ❌ Pendiente
- ❌ Autenticación y autorización (JWT)
- ❌ Generación de PDFs (tickets, reportes)
- ❌ Reportes con filtros
- ❌ Envío de correos electrónicos
- ❌ WebSockets para chat
- ❌ Dashboard con gráficas
- ❌ Recuperación de contraseña
- ❌ Notificaciones de inventario bajo

---

## 🗓️ FASES DEL PROYECTO

---

## **FASE 1: Completar y Validar Entidades** ✅ (Mayormente completado)

### Objetivos
- Revisar y completar todas las entidades del dominio
- Asegurar que las relaciones estén correctamente definidas
- Validar las migraciones de base de datos

### Tareas
- [x] Entidad `User` con relación a `Role`
- [x] Entidad `Customer`
- [x] Entidad `Product`
- [x] Entidad `Inventory` con relación a `Product`
- [x] Entidad `Sale` con relaciones a `Customer` y `User`
- [x] Entidad `SaleDetail` con relaciones a `Sale` y `Product`
- [ ] **NUEVA**: Entidad `CashRegister` (para cortes de caja)
- [ ] **NUEVA**: Entidad `InventoryMovement` (historial de movimientos de inventario)
- [ ] **NUEVA**: Entidad `PasswordResetToken` (para recuperación de contraseña)
- [ ] **NUEVA**: Entidad `EmailLog` (registro de correos enviados)
- [ ] **NUEVA**: Entidad `ChatMessage` (para el chat entre usuarios)

### Entregables
- Todas las entidades del dominio completas
- Migraciones aplicadas correctamente
- Documentación de las entidades en el código

---

## **FASE 2: Fundamentos del Proyecto** ✅ (Completado)

Esta fase ya está completada. Tu proyecto tiene:
- Clean Architecture con separación de capas
- CQRS implementado
- Repository Pattern
- Unit of Work
- Mediador personalizado
- Result Pattern para manejo de errores

---

## **FASE 3: Completar Lógica de Negocio Core** 🔄

### Objetivos
- Implementar toda la lógica de negocio para las operaciones principales
- Asegurar la integridad de datos en operaciones críticas
- Implementar transacciones donde sea necesario

### 3.1 Módulo de Ventas (CRÍTICO)
**Tiempo estimado: No proporcionaré estimaciones, pero esta es una prioridad alta**

#### Tareas
- [ ] Implementar `CreateSaleHandler` con las siguientes validaciones:
  - Verificar que el cliente existe
  - Verificar que todos los productos existen
  - Verificar que hay suficiente inventario para cada producto
  - Calcular totales (subtotal, impuestos, descuentos, total)
  - Usar transacciones para asegurar atomicidad
- [ ] Implementar descuento automático de inventario al crear venta:
  ```csharp
  // Pseudocódigo
  foreach (var detail in saleDetails)
  {
      var inventory = await GetInventory(detail.ProductId);
      inventory.Quantity -= detail.Quantity;
      _unitOfWork.Repository<Inventory>().Update(inventory);
  }
  ```
- [ ] Crear `InventoryMovement` por cada cambio de inventario
- [ ] Implementar validación de stock mínimo (< 10 unidades)
- [ ] Implementar cancelación de ventas (con reintegro de inventario)
- [ ] Crear `SaleSpecification` para consultas complejas de ventas

#### Comandos y Queries necesarios
```
Commands:
- CreateSaleCommand ✅ (mejorar)
- CancelSaleCommand (nuevo)

Queries:
- SaleGetByIdQuery ✅
- SaleGetAllQuery ✅
- SalesGetByDateRangeQuery (nuevo)
- SalesGetByCustomerQuery (nuevo)
- SalesGetDailySummaryQuery (nuevo)
```

### 3.2 Módulo de Inventario
#### Tareas
- [ ] Implementar `UpdateInventoryCommand` (ajustes manuales)
- [ ] Implementar `InventoryMovementCreateCommand`
- [ ] Crear `InventoryGetLowStockQuery` (productos con < 10 unidades)
- [ ] Implementar especificación para inventarios bajos
- [ ] Crear historial de movimientos de inventario

### 3.3 Módulo de Productos
#### Tareas
- [ ] Verificar CRUDs completos
- [ ] Implementar `ProductGetByCategoryQuery` (si tienes categorías)
- [ ] Implementar búsqueda de productos por nombre/código

### 3.4 Módulo de Clientes
#### Tareas
- [ ] Verificar CRUDs completos
- [ ] Implementar `CustomerGetByPhoneQuery`
- [ ] Implementar `CustomerGetPurchaseHistoryQuery`

### 3.5 Módulo de Usuarios
#### Tareas
- [ ] Implementar hash de contraseñas usando `IEncryptionService`
- [ ] Verificar que no se pueden crear usuarios duplicados (email único)
- [ ] Implementar `UserGetByEmailQuery`

---

## **FASE 4: Autenticación y Autorización** 🔐

### Objetivos
- Implementar JWT con Access Token y Refresh Token
- Implementar roles y permisos
- Proteger endpoints con autorización basada en roles

### 4.1 JWT Authentication
#### Tareas
- [ ] Instalar paquetes NuGet:
  ```bash
  dotnet add src/Web.API package Microsoft.AspNetCore.Authentication.JwtBearer
  dotnet add src/Infrastructure package System.IdentityModel.Tokens.Jwt
  ```
- [ ] Crear configuración de JWT en `appsettings.json`:
  ```json
  "JwtSettings": {
    "Secret": "tu-secreto-super-seguro-de-al-menos-32-caracteres",
    "Issuer": "SuperPOS",
    "Audience": "SuperPOS-API",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
  ```
- [ ] Crear `JwtSettings` class en Application/Common
- [ ] Crear `IJwtService` interface en Application/Interfaces/Services
- [ ] Implementar `JwtService` en Infrastructure/Services:
  - Método `GenerateAccessToken(User user)`
  - Método `GenerateRefreshToken()`
  - Método `ValidateToken(string token)`
  - Método `GetPrincipalFromExpiredToken(string token)`
- [ ] Crear entidad `RefreshToken` en Domain:
  - Token (string)
  - UserId (Guid)
  - ExpiresAt (DateTime)
  - CreatedAt (DateTime)
  - RevokedAt (DateTime?)
- [ ] Implementar `LoginCommand` y `LoginHandler`:
  - Validar credenciales
  - Generar Access Token
  - Generar Refresh Token
  - Guardar Refresh Token en BD
- [ ] Implementar `RefreshTokenCommand` y `RefreshTokenHandler`:
  - Validar Refresh Token
  - Generar nuevo Access Token
  - Rotar Refresh Token (generar nuevo)
- [ ] Crear `AuthController` con endpoints:
  - POST `/api/auth/login`
  - POST `/api/auth/refresh`
  - POST `/api/auth/logout`
- [ ] Configurar JWT en `Program.cs` o extensión de DI

### 4.2 Autorización basada en Roles
#### Tareas
- [ ] Crear `[Authorize]` attribute en controllers
- [ ] Crear política de autorización para roles:
  - Administrador: acceso total
  - Gerente: acceso a reportes, inventario, usuarios
  - Vendedor: acceso solo a ventas y consultas
- [ ] Implementar `[Authorize(Roles = "Administrador,Gerente")]` en endpoints críticos
- [ ] Crear middleware o filtro para validar permisos granulares
- [ ] Documentar qué rol puede acceder a qué endpoint

#### Ejemplo de endpoints con roles
```csharp
// Solo Administrador y Gerente
[Authorize(Roles = "Administrador,Gerente")]
[HttpPost("users")]
public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command) { }

// Todos los roles autenticados
[Authorize]
[HttpGet("products")]
public async Task<IActionResult> GetAllProducts() { }

// Solo Administrador
[Authorize(Roles = "Administrador")]
[HttpDelete("users/{id}")]
public async Task<IActionResult> DeleteUser(Guid id) { }
```

---

## **FASE 5: Generación de PDFs** 📄

### Objetivos
- Generar tickets de venta en PDF
- Generar corte de caja en PDF
- Crear reportes de ventas en PDF

### 5.1 Configuración inicial
#### Tareas
- [ ] Evaluar librería a usar:
  - **QuestPDF** (recomendado - moderno, fluent API, gratis para uso comercial)
  - iTextSharp (antiguo pero robusto)
  - PdfSharpCore (open source)
- [ ] Instalar QuestPDF:
  ```bash
  dotnet add src/Infrastructure package QuestPDF
  ```
- [ ] Crear `IPdfService` interface en Application/Interfaces/Services
- [ ] Implementar `PdfService` en Infrastructure/Services

### 5.2 Ticket de Venta
#### Tareas
- [ ] Crear `GenerateSaleTicketCommand(Guid SaleId)`
- [ ] Implementar `GenerateSaleTicketHandler`:
  - Obtener venta con detalles
  - Llamar a `IPdfService.GenerateSaleTicket(sale)`
  - Retornar PDF como byte[]
- [ ] Diseñar layout del ticket:
  - Logo/nombre del negocio
  - Fecha y hora
  - Número de ticket
  - Datos del cliente
  - Tabla de productos (producto, cantidad, precio, subtotal)
  - Subtotal, impuestos, descuentos, total
  - Método de pago
  - Mensaje de agradecimiento
- [ ] Crear endpoint `GET /api/sales/{id}/ticket` que retorne el PDF
- [ ] Retornar PDF con header correcto:
  ```csharp
  return File(pdfBytes, "application/pdf", $"Ticket-{saleId}.pdf");
  ```

### 5.3 Corte de Caja (End of Day Report)
#### Tareas
- [ ] Crear entidad `CashRegister`:
  - Id
  - UserId (quien hace el corte)
  - OpeningDate
  - ClosingDate
  - InitialCash (efectivo inicial)
  - TotalCash (efectivo al cierre)
  - TotalSales (total de ventas del día)
  - TotalTransactions (número de transacciones)
  - Notes
- [ ] Crear `GenerateCashRegisterReportCommand(DateTime date)`
- [ ] Implementar handler que:
  - Obtenga todas las ventas del día
  - Calcule totales por método de pago
  - Genere PDF con resumen
- [ ] Diseñar layout del corte:
  - Fecha del corte
  - Usuario que cierra
  - Resumen de ventas (cantidad, total)
  - Desglose por método de pago
  - Efectivo inicial vs final
  - Diferencias (si las hay)
- [ ] Crear endpoint `POST /api/cash-register/close`

### 5.4 Reportes de Ventas
#### Tareas
- [ ] Crear `GenerateSalesReportQuery`:
  - DateFrom, DateTo (rango de fechas)
  - CustomerId (opcional - filtrar por cliente)
  - GroupBy (día, semana, mes)
- [ ] Implementar handler que genere PDF con:
  - Gráficas de ventas (usar librería de charts)
  - Tabla de ventas
  - Totales y promedios
  - Top productos vendidos
- [ ] Crear endpoint `GET /api/reports/sales?dateFrom=...&dateTo=...&customerId=...`

---

## **FASE 6: Reportes Avanzados (PDF/CSV)** 📊

### Objetivos
- Generar reportes con múltiples filtros
- Exportar datos en PDF y CSV
- Implementar análisis de ventas

### 6.1 Exportación a CSV
#### Tareas
- [ ] Instalar CsvHelper:
  ```bash
  dotnet add src/Infrastructure package CsvHelper
  ```
- [ ] Crear `ICsvExportService` interface
- [ ] Implementar `CsvExportService`
- [ ] Crear `ExportSalesToCsvQuery` con los mismos filtros que PDF
- [ ] Implementar handler que genere CSV
- [ ] Crear endpoint `GET /api/reports/sales/export?format=csv&dateFrom=...`

### 6.2 Filtros Avanzados
#### Tareas
- [ ] Implementar filtro por rango de fechas
- [ ] Implementar filtro por día de la semana:
  ```csharp
  var sales = await _unitOfWork.Repository<Sale>()
      .QueryAsync(s => s.CreatedAt.DayOfWeek == DayOfWeek.Monday);
  ```
- [ ] Implementar filtro por mes:
  ```csharp
  var sales = await _unitOfWork.Repository<Sale>()
      .QueryAsync(s => s.CreatedAt.Month == month && s.CreatedAt.Year == year);
  ```
- [ ] Implementar filtro por cliente
- [ ] Implementar filtro por producto
- [ ] Implementar filtro por vendedor (User)
- [ ] Crear specification `SalesReportSpecification` que combine todos los filtros

### 6.3 Análisis de Ventas
#### Tareas
- [ ] Crear query para ventas por período
- [ ] Crear query para productos más vendidos
- [ ] Crear query para clientes frecuentes
- [ ] Crear query para comparación de períodos (mes actual vs mes anterior)

---

## **FASE 7: Dashboard con Gráficas** 📈

### Objetivos
- Crear endpoint para datos del dashboard
- Proporcionar métricas clave del negocio
- Datos listos para consumir por frontend

### 7.1 Métricas del Dashboard
#### Tareas
- [ ] Crear `DashboardGetMetricsQuery(DateTime? startDate, DateTime? endDate)`
- [ ] Implementar handler que retorne:
  - **Ventas del día**: total de ventas hoy
  - **Ventas del mes**: total de ventas del mes actual
  - **Comparación con mes anterior**: % de crecimiento/decrecimiento
  - **Productos con stock bajo**: cantidad de productos con < 10 unidades
  - **Top 5 productos más vendidos**
  - **Top 5 clientes frecuentes**
  - **Ventas por día (últimos 30 días)**: para gráfica de línea
  - **Ventas por método de pago**: para gráfica de pie
  - **Ventas por vendedor**: para ranking
- [ ] Crear `DashboardMetricsDTO` con todas las métricas
- [ ] Crear endpoint `GET /api/dashboard/metrics`

### 7.2 Gráficas Recomendadas
```
1. Línea temporal: Ventas de los últimos 30 días
2. Pie chart: Distribución por método de pago
3. Barras: Top 10 productos más vendidos
4. Tarjetas (cards):
   - Total ventas del día
   - Total ventas del mes
   - Productos con stock bajo
   - Número de clientes
5. Tabla: Últimas ventas realizadas
```

---

## **FASE 8: Sistema de Notificaciones (Email)** 📧

### Objetivos
- Implementar envío de correos electrónicos
- Notificar cuando el inventario esté bajo
- Sentar las bases para recuperación de contraseña

### 8.1 Configuración de Email
#### Tareas
- [ ] Elegir proveedor de email:
  - **SendGrid** (recomendado - 100 emails/día gratis)
  - MailKit (SMTP directo)
  - AWS SES
- [ ] Instalar paquete:
  ```bash
  dotnet add src/Infrastructure package SendGrid
  # O si usas MailKit:
  dotnet add src/Infrastructure package MailKit
  ```
- [ ] Configurar credenciales en `appsettings.json`:
  ```json
  "EmailSettings": {
    "FromEmail": "noreply@superpos.com",
    "FromName": "SuperPOS",
    "SendGridApiKey": "tu-api-key"
  }
  ```
- [ ] Crear `IEmailService` interface en Application/Interfaces/Services:
  - `Task SendEmailAsync(string to, string subject, string htmlBody)`
  - `Task SendTemplateEmailAsync(string to, string templateId, object data)`
- [ ] Implementar `EmailService` en Infrastructure/Services

### 8.2 Notificación de Inventario Bajo
#### Tareas
- [ ] Crear `CheckLowStockCommand` (se ejecutará periódicamente)
- [ ] Implementar handler que:
  - Obtenga productos con inventario < 10
  - Obtenga usuarios con rol "Gerente"
  - Envíe email a cada gerente con la lista de productos
- [ ] Crear template HTML para el email:
  ```html
  <h1>Alerta de Inventario Bajo</h1>
  <p>Los siguientes productos tienen menos de 10 unidades:</p>
  <ul>
    <li>Producto A - 5 unidades</li>
    <li>Producto B - 3 unidades</li>
  </ul>
  ```
- [ ] Implementar job recurrente (ver FASE 8.3)
- [ ] Crear registro de emails enviados (entidad `EmailLog`)

### 8.3 Background Jobs (Hangfire)
#### Tareas
- [ ] Instalar Hangfire:
  ```bash
  dotnet add src/Web.API package Hangfire.AspNetCore
  dotnet add src/Infrastructure package Hangfire.SqlServer
  ```
- [ ] Configurar Hangfire en `Program.cs`
- [ ] Crear job recurrente para verificar inventario bajo:
  ```csharp
  RecurringJob.AddOrUpdate(
      "check-low-stock",
      () => mediator.Send(new CheckLowStockCommand()),
      Cron.Daily(9) // Ejecutar diariamente a las 9 AM
  );
  ```
- [ ] Configurar Hangfire Dashboard (solo para Administradores)

---

## **FASE 9: Recuperación de Contraseña** 🔑

### Objetivos
- Implementar flujo de recuperación de contraseña por email
- (Opcional) Implementar recuperación por WhatsApp

### 9.1 Recuperación por Email
#### Tareas
- [ ] Crear entidad `PasswordResetToken`:
  - Id
  - UserId
  - Token (string único)
  - ExpiresAt (DateTime)
  - UsedAt (DateTime?)
- [ ] Crear `RequestPasswordResetCommand(string email)`
- [ ] Implementar handler:
  - Verificar que el usuario existe
  - Generar token único (6 dígitos o GUID)
  - Guardar token en BD con expiración (15 minutos)
  - Enviar email con el token/link
- [ ] Crear `ResetPasswordCommand(string token, string newPassword)`
- [ ] Implementar handler:
  - Validar que el token existe y no ha expirado
  - Validar que no se ha usado
  - Hashear nueva contraseña
  - Actualizar contraseña del usuario
  - Marcar token como usado
- [ ] Crear endpoints:
  - POST `/api/auth/forgot-password` (envía email)
  - POST `/api/auth/reset-password` (cambia contraseña)

### 9.2 Recuperación por WhatsApp (Opcional)
#### Tareas
- [ ] Elegir proveedor:
  - Twilio (recomendado)
  - WhatsApp Business API
- [ ] Instalar Twilio SDK:
  ```bash
  dotnet add src/Infrastructure package Twilio
  ```
- [ ] Crear `IWhatsAppService` interface
- [ ] Implementar `WhatsAppService`
- [ ] Modificar `RequestPasswordResetCommand` para aceptar método (email o whatsapp)
- [ ] Enviar código de 6 dígitos por WhatsApp

---

## **FASE 10: WebSockets para Chat** 💬

### Objetivos
- Implementar chat en tiempo real entre usuarios
- Restringir chat: Gerente/Admin ↔ Vendedor (no Vendedor ↔ Vendedor)

### 10.1 Configuración de WebSockets
#### Tareas
- [ ] Crear entidad `ChatMessage`:
  - Id
  - SenderId (User)
  - ReceiverId (User)
  - Message (string)
  - SentAt (DateTime)
  - ReadAt (DateTime?)
- [ ] Crear `ChatMessageDTO`
- [ ] Instalar SignalR (ya incluido en ASP.NET Core)
- [ ] Crear `ChatHub` en Web.API/Hubs:
  ```csharp
  public class ChatHub : Hub
  {
      public async Task SendMessage(string receiverId, string message) { }
      public async Task JoinRoom(string userId) { }
      public override async Task OnConnectedAsync() { }
      public override async Task OnDisconnectedAsync(Exception ex) { }
  }
  ```
- [ ] Configurar SignalR en `Program.cs`:
  ```csharp
  builder.Services.AddSignalR();
  app.MapHub<ChatHub>("/chatHub");
  ```

### 10.2 Lógica de Chat
#### Tareas
- [ ] Implementar validación de roles en `ChatHub`:
  - Obtener rol del sender
  - Obtener rol del receiver
  - Permitir solo si:
    - Sender es Gerente/Admin Y Receiver es Vendedor
    - Sender es Vendedor Y Receiver es Gerente/Admin
  - Denegar si ambos son Vendedores
- [ ] Guardar mensajes en BD
- [ ] Implementar notificaciones de mensajes no leídos
- [ ] Crear queries:
  - `GetChatHistoryQuery(Guid userId1, Guid userId2)`
  - `GetUnreadMessagesCountQuery(Guid userId)`
  - `GetActiveChatsQuery(Guid userId)` (lista de conversaciones)
- [ ] Crear endpoints REST adicionales:
  - GET `/api/chat/history/{userId}` (obtener historial)
  - GET `/api/chat/unread` (obtener mensajes no leídos)
  - PUT `/api/chat/messages/{id}/read` (marcar como leído)

### 10.3 Gestión de Conexiones
#### Tareas
- [ ] Implementar diccionario de conexiones activas:
  ```csharp
  private static readonly Dictionary<string, string> _connections = new();
  // Key: UserId, Value: ConnectionId
  ```
- [ ] Actualizar conexiones en `OnConnectedAsync` y `OnDisconnectedAsync`
- [ ] Implementar presencia (quién está en línea)
- [ ] Enviar eventos de "escribiendo..." (typing indicators)

---

## **FASE 11: Testing y Calidad de Código** 🧪

### Objetivos
- Implementar pruebas unitarias
- Implementar pruebas de integración
- Asegurar la calidad del código

### 11.1 Pruebas Unitarias
#### Tareas
- [ ] Crear proyecto de pruebas:
  ```bash
  dotnet new xunit -n Tests.Unit -o tests/Tests.Unit
  dotnet add tests/Tests.Unit package FluentAssertions
  dotnet add tests/Tests.Unit package Moq
  dotnet sln add tests/Tests.Unit
  ```
- [ ] Escribir tests para handlers críticos:
  - `CreateSaleHandlerTests`
  - `LoginHandlerTests`
  - `GenerateSaleTicketHandlerTests`
- [ ] Escribir tests para servicios:
  - `JwtServiceTests`
  - `PdfServiceTests`
  - `EmailServiceTests`
- [ ] Objetivo: >70% de cobertura en capa Application

### 11.2 Pruebas de Integración
#### Tareas
- [ ] Crear proyecto de pruebas de integración:
  ```bash
  dotnet new xunit -n Tests.Integration -o tests/Tests.Integration
  dotnet add tests/Tests.Integration package Microsoft.AspNetCore.Mvc.Testing
  ```
- [ ] Crear `WebApplicationFactory` personalizado
- [ ] Escribir tests de endpoints:
  - POST `/api/auth/login`
  - POST `/api/sales`
  - GET `/api/dashboard/metrics`
- [ ] Usar base de datos en memoria o contenedor Docker para tests

### 11.3 Calidad de Código
#### Tareas
- [ ] Configurar analizadores de código:
  ```bash
  dotnet add package Microsoft.CodeAnalysis.NetAnalyzers
  ```
- [ ] Crear `.editorconfig` con reglas de estilo
- [ ] Ejecutar análisis estático:
  ```bash
  dotnet format --verify-no-changes
  ```
- [ ] Revisar y eliminar code smells

---

## **FASE 12: Documentación y DevOps** 📚

### Objetivos
- Documentar la API
- Configurar CI/CD
- Preparar para deployment

### 12.1 Documentación de API
#### Tareas
- [ ] Mejorar documentación de Swagger:
  - Agregar descripciones a endpoints
  - Documentar códigos de respuesta
  - Agregar ejemplos de requests/responses
- [ ] Crear archivo Postman Collection
- [ ] Documentar variables de entorno necesarias
- [ ] Crear diagrama de arquitectura
- [ ] Documentar flujos principales (venta, login, etc.)

### 12.2 CI/CD
#### Tareas
- [ ] Crear pipeline de GitHub Actions o GitLab CI:
  ```yaml
  - Restore dependencies
  - Build
  - Run tests
  - Publish artifacts
  ```
- [ ] Configurar análisis de cobertura
- [ ] Configurar deployment automático a staging

### 12.3 Dockerización
#### Tareas
- [ ] Crear `Dockerfile` para la API
- [ ] Crear `docker-compose.yml` con API + SQL Server
- [ ] Documentar cómo ejecutar con Docker

---

## 💡 FUNCIONALIDADES ADICIONALES SUGERIDAS

### 1. **Gestión de Proveedores (Suppliers)**
**¿Para qué?** Registrar de quién compras los productos y llevar control de compras.

**Implementación:**
- Entidad `Supplier` (nombre, contacto, email, dirección)
- Entidad `PurchaseOrder` (orden de compra a proveedor)
- CRUD de proveedores
- Generar reporte de compras por proveedor

---

### 2. **Múltiples Métodos de Pago en una Venta**
**¿Para qué?** Un cliente puede pagar parte en efectivo y parte con tarjeta.

**Implementación:**
- Entidad `PaymentMethod` (efectivo, tarjeta, transferencia)
- Entidad `SalePayment` (relaciona Sale con PaymentMethod y monto)
- Una venta puede tener múltiples `SalePayment`
- Validar que la suma de pagos = total de venta

---

### 3. **Descuentos y Promociones**
**¿Para qué?** Aplicar descuentos automáticos (2x1, descuento por cantidad, cupones).

**Implementación:**
- Entidad `Discount` (tipo, valor, fecha inicio/fin, condiciones)
- Entidad `DiscountRule` (reglas de aplicación: "compra 2 lleva 3")
- Aplicar descuentos en el handler de `CreateSale`
- Motor de reglas para evaluar si aplica descuento

---

### 4. **Categorías de Productos**
**¿Para qué?** Organizar productos y generar reportes por categoría.

**Implementación:**
- Entidad `Category` (nombre, descripción)
- Relación Product → Category
- Filtros por categoría en productos
- Reporte de ventas por categoría

---

### 5. **Devoluciones y Cambios**
**¿Para qué?** Manejar productos devueltos y reintegrar al inventario.

**Implementación:**
- Entidad `Return` (venta original, productos devueltos, motivo)
- Comando `CreateReturnCommand`
- Reintegrar productos al inventario
- Generar nota de crédito

---

### 6. **Cuentas por Cobrar (Ventas a Crédito)**
**¿Para qué?** Permitir ventas a crédito y llevar control de pagos pendientes.

**Implementación:**
- Campo `PaymentStatus` en Sale (Paid, Pending, Partial)
- Entidad `Payment` (abonos parciales a una venta)
- Query para obtener cuentas por cobrar
- Alertas de pagos vencidos

---

### 7. **Multi-Tienda / Multi-Sucursal**
**¿Para qué?** Administrar múltiples puntos de venta.

**Implementación:**
- Entidad `Store` (sucursal)
- Relación User → Store (usuarios asignados a sucursal)
- Inventario por sucursal
- Reportes por sucursal
- Transferencias de inventario entre sucursales

---

### 8. **Códigos de Barras / QR**
**¿Para qué?** Escanear productos para agilizar ventas.

**Implementación:**
- Campo `Barcode` en Product
- Endpoint `GET /api/products/by-barcode/{barcode}`
- Generar códigos QR para productos (usar QRCoder library)
- Imprimir etiquetas con código de barras

---

### 9. **Auditoría de Cambios**
**¿Para qué?** Saber quién modificó qué y cuándo.

**Implementación:**
- Patrón Audit Trail
- Entidad `AuditLog` (tabla, registro, acción, usuario, fecha, valores anteriores/nuevos)
- Interceptor de EF Core para registrar cambios automáticamente
- Endpoint para consultar auditoría

---

### 10. **Integraciones Externas**
**¿Para qué?** Conectar con otros sistemas.

**Opciones:**
- **Facturación electrónica** (SAT en México, SUNAT en Perú, etc.)
- **Pasarelas de pago** (Stripe, PayPal, Mercado Pago)
- **Contabilidad** (QuickBooks, SAP)
- **Logística** (APIs de mensajería)

---

### 11. **Progressive Web App (PWA) para el POS**
**¿Para qué?** Usar el sistema offline cuando no hay internet.

**Implementación:**
- Configurar Service Workers
- Cache de productos y precios
- Sincronización al recuperar conexión
- Requiere frontend (Vue, React, Angular)

---

### 12. **Métricas Avanzadas y Machine Learning**
**¿Para qué?** Predicciones y análisis inteligente.

**Ideas:**
- Predicción de demanda (qué productos se venderán más)
- Detección de anomalías (fraude, pérdidas)
- Recomendación de productos (qué comprar junto)
- Optimización de inventario (cuándo reabastecer)

**Herramientas:**
- ML.NET (Microsoft)
- Python con integración a .NET (gRPC)

---

### 13. **Modo Kiosco / Auto-Checkout**
**¿Para qué?** Que los clientes se atiendan solos.

**Implementación:**
- Interfaz simplificada para clientes
- Escaneo de productos por el cliente
- Pago con tarjeta/QR automático
- Generación de ticket digital

---

### 14. **Gamificación para Vendedores**
**¿Para qué?** Motivar a los vendedores con rankings y recompensas.

**Implementación:**
- Entidad `Achievement` (logros)
- Sistema de puntos por ventas
- Leaderboard (ranking de vendedores)
- Notificaciones de logros desbloqueados

---

### 15. **API Pública / Webhooks**
**¿Para qué?** Permitir integraciones de terceros.

**Implementación:**
- Crear API Keys para clientes externos
- Documentar API con OpenAPI
- Implementar webhooks para eventos (nueva venta, producto agotado)
- Rate limiting para evitar abuso

---

## 🎯 RECOMENDACIONES FINALES

### Priorización Sugerida

#### **MUST HAVE (Imprescindible para MVP)**
1. ✅ Autenticación y autorización (FASE 4)
2. ✅ Lógica de ventas completa con descuento de inventario (FASE 3)
3. ✅ Generación de ticket de venta PDF (FASE 5.2)
4. ✅ Dashboard básico con métricas (FASE 7)
5. ✅ Reportes básicos de ventas (FASE 6)

#### **SHOULD HAVE (Importante, pero puede esperar)**
6. Notificaciones de inventario bajo por email (FASE 8)
7. Recuperación de contraseña (FASE 9)
8. Corte de caja PDF (FASE 5.3)
9. Exportación a CSV (FASE 6.1)
10. WebSockets para chat (FASE 10)

#### **NICE TO HAVE (Extras para aprender)**
11. Múltiples métodos de pago
12. Categorías de productos
13. Códigos de barras
14. Auditoría de cambios
15. Testing completo

---

### Tecnologías Recomendadas por Área

| Área | Tecnología | Razón |
|------|------------|-------|
| **Autenticación** | JWT + RefreshToken | Estándar de la industria, stateless |
| **PDFs** | QuestPDF | API moderna y fluida, fácil de usar |
| **Email** | SendGrid | Tier gratuito generoso, confiable |
| **Background Jobs** | Hangfire | Integración perfecta con .NET, dashboard incluido |
| **WebSockets** | SignalR | Nativo de ASP.NET Core, fácil de usar |
| **Reportes** | QuestPDF + CsvHelper | Flexible para PDF y CSV |
| **Testing** | xUnit + FluentAssertions + Moq | Stack estándar para .NET |
| **WhatsApp** | Twilio | API robusta, bien documentada |
| **Cache** | Redis | Alto rendimiento, escalable |
| **Logging** | Serilog | Estructurado, múltiples sinks |

---

### Consejos para el Aprendizaje

1. **No implementes todo a la vez**: Ve fase por fase, terminando completamente una antes de pasar a la siguiente.

2. **Testea cada funcionalidad**: Antes de marcar una tarea como completa, prueba casos normales y casos extremos.

3. **Documenta mientras desarrollas**: Agrega comentarios XML en clases y métodos importantes.

4. **Haz commits pequeños y descriptivos**: Facilita volver atrás si algo sale mal.

5. **Refactoriza constantemente**: Si ves código repetido, extrae métodos o clases.

6. **Pregunta cuando te atores**: Usa ChatGPT, Claude o foros de .NET cuando tengas dudas.

7. **Revisa el código de proyectos open source**: Aprende de cómo otros resuelven problemas similares.

8. **Mide el rendimiento**: Usa herramientas como BenchmarkDotNet o Application Insights para identificar cuellos de botella.

---

### Recursos de Aprendizaje

#### Documentación Oficial
- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [SignalR](https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction)

#### Libros Recomendados
- "Clean Architecture" - Robert C. Martin
- "Domain-Driven Design" - Eric Evans
- "Patterns of Enterprise Application Architecture" - Martin Fowler

#### Cursos
- Pluralsight: ASP.NET Core Path
- Udemy: "Complete Guide to ASP.NET Core"
- YouTube: Nick Chapsas (excelente canal de .NET)

#### Repositorios de Referencia
- [eShopOnWeb](https://github.com/dotnet-architecture/eShopOnWeb) - Clean Architecture de Microsoft
- [Ardalis CleanArchitecture](https://github.com/ardalis/CleanArchitecture) - Template de Clean Architecture

---

## 📝 Checklist de Progreso

Marca cada tarea a medida que la completes:

### Fase 1: Entidades
- [x] Entidades base (User, Role, Customer, Product, Inventory, Sale, SaleDetail)
- [ ] CashRegister
- [ ] InventoryMovement
- [ ] PasswordResetToken
- [ ] EmailLog
- [ ] ChatMessage

### Fase 3: Lógica de Negocio
- [ ] Ventas con descuento automático de inventario
- [ ] Validación de stock
- [ ] Cancelación de ventas
- [ ] Historial de movimientos de inventario

### Fase 4: Auth
- [ ] JWT con Access Token
- [ ] Refresh Token
- [ ] Login/Logout
- [ ] Autorización basada en roles

### Fase 5: PDFs
- [ ] Ticket de venta
- [ ] Corte de caja
- [ ] Reportes de ventas

### Fase 6: Reportes
- [ ] Filtros por fechas
- [ ] Filtro por cliente
- [ ] Exportación a CSV

### Fase 7: Dashboard
- [ ] Métricas del día/mes
- [ ] Top productos
- [ ] Productos con stock bajo

### Fase 8: Emails
- [ ] Configuración de email service
- [ ] Notificación de inventario bajo
- [ ] Background jobs con Hangfire

### Fase 9: Recuperación de Contraseña
- [ ] Flujo completo por email
- [ ] (Opcional) Por WhatsApp

### Fase 10: WebSockets
- [ ] Chat en tiempo real
- [ ] Validación de roles
- [ ] Historial de mensajes

### Fase 11: Testing
- [ ] Pruebas unitarias
- [ ] Pruebas de integración
- [ ] >70% cobertura

### Fase 12: DevOps
- [ ] Documentación completa
- [ ] CI/CD pipeline
- [ ] Dockerización

---

## 🎉 Conclusión

Este plan de trabajo te llevará desde donde estás ahora hasta un sistema de punto de venta completo y robusto. No solo aprenderás las tecnologías, sino también mejores prácticas de arquitectura de software, patrones de diseño y DevOps.

**Recuerda**: El objetivo no es solo terminar el proyecto, sino **aprender en el proceso**. Tómate el tiempo para entender cada concepto y no dudes en experimentar.

**¡Mucho éxito con tu proyecto SuperPOS!** 🚀
