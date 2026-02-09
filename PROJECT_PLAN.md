# PLAN DE TRABAJO - SuperPOS

> **⚠️ REGLA IMPORTANTE**: Este documento es únicamente el plan de trabajo del proyecto. **Ninguna IA debe modificar este documento a menos que sea solicitado explícitamente por el usuario.** El plan describe lo que se debe implementar, no lo que ya está implementado.

---

## Propósito del Proyecto

Este proyecto es un **sistema POS (Point of Sale)** completo desarrollado con **.NET 10** siguiendo **Clean Architecture** y **Domain-Driven Design (DDD)**. El objetivo principal es aprender e implementar conceptos avanzados de desarrollo de software a través de un proyecto práctico y completo.

---

## Plan de Implementación

### 1. Definir las Entidades

Definir todas las entidades del dominio que formarán parte del sistema. Todas deben estar contempladas desde el principio, aunque su implementación ocurra en diferentes fases del proyecto:

- **Product** (Producto): Información del producto, precio, código de barras, stock, etc.
- **Customer** (Cliente): Datos del cliente que realiza compras
- **User** (Usuario): Usuarios del sistema con diferentes roles
- **Role** (Rol): Roles para control de acceso (Administrador, Gerente, Vendedor)
- **Sale** (Venta): Transacción de venta con sus detalles (Aggregate Root)
- **SaleDetail** (Detalle de Venta): Items individuales de una venta (parte del agregado Sale)
- **Inventory** (Inventario): Control de stock de productos
- **InventoryOperation** (Operación de Inventario): Historial de movimientos de inventario (Add, Set, Remove)
- **CashRegister** (Caja Registradora): Registro de apertura/cierre de caja, movimientos del día (se implementa en Fase 5)
- **EmailLog** (Registro de Emails): Auditoría de correos enviados - stock bajo, recuperación contraseña, etc. (se implementa en Fase 8)
- **RefreshToken** (Token de Refresco): Almacenamiento de refresh tokens JWT para renovación de sesión (se implementa en Fase 9)
- **PasswordResetToken** (Token de Recuperación): Códigos temporales para recuperación de contraseña (se implementa en Fase 11)
- **ChatMessage** (Mensaje de Chat): Mensajes del chat en tiempo real entre usuarios (se implementa en Fase 12)
- **Conversation** (Conversación): Agrupación de mensajes entre dos usuarios (se implementa en Fase 12)

### 2. Crear la Base del Proyecto usando Clean Architecture + DDD

Estructura del proyecto en capas:

- **Domain**: Entidades, Value Objects, Domain Events, Repositorios (interfaces)
- **Application**: Casos de uso (CQRS), DTOs, Servicios de aplicación, Validaciones
- **Infrastructure**: Implementación de repositorios, EF Core, DbContext, Servicios externos
- **Web.API**: Controllers, Middleware, Configuración de API

Implementar:
- Patrón Repository
- Unit of Work
- Specification Pattern
- CQRS (Command Query Responsibility Segregation)
- Domain Events
- Value Objects (Money, Email, PersonName, PhoneNumber, Barcode, Quantity)

### 3. Implementar el Comportamiento de las Entidades

#### CRUDs básicos para:
- Productos (CRUD completo con búsqueda por nombre y código de barras)
- Clientes (CRUD completo con búsqueda)
- Usuarios (CRUD completo con encriptación de contraseñas)
- Roles (CRUD completo)
- Inventario (Ajustes de stock: agregar, establecer, quitar)

#### Funcionalidad de Ventas:
- Crear venta con múltiples productos
- Validar existencia de productos, clientes y usuarios
- **Descuento de inventario automático al realizar una venta**
- Sistema de reserva de stock en dos fases (validar/reservar → commit/rollback)
- Validación de stock suficiente antes de completar venta
- Protección de invariantes del agregado Sale

#### Reglas de Negocio:
- No permitir stock negativo
- Validar unicidad de códigos de barras
- Validar que las ventas tengan al menos un producto
- Validar que el total de la venta coincida con la suma de los detalles

### 4. Generar PDF con el Ticket de Venta

Implementar generación de ticket de venta en formato PDF:

- Información del negocio (nombre, dirección, teléfono)
- Fecha y hora de la venta
- Número de ticket
- Datos del cliente
- Detalles de productos (nombre, cantidad, precio unitario, subtotal)
- Total de la venta
- Usuario que procesó la venta
- Mensaje de agradecimiento

**Tecnologías sugeridas**: QuestPDF, iTextSharp, o similar

### 5. Generar Ticket de Corte de Caja

Generar un reporte PDF que muestre todos los movimientos del día:

- Fecha del corte
- Usuario que realiza el corte
- Ventas del día (cantidad y monto total)
- Desglose por método de pago (efectivo, tarjeta, etc.)
- Total de ingresos
- Gastos del día (si aplica)
- Saldo final de caja
- Firma del responsable

### 6. Generar Reporte de Ventas con Filtros

Implementar sistema de reportes con múltiples filtros:

#### Filtros disponibles:
- Rango de fechas (fecha inicio - fecha fin)
- Por días de la semana (Lunes, Martes, etc.)
- Por mes específico
- Por cliente específico
- Combinación de filtros

#### Formatos de salida:
- **PDF**: Reporte visual con gráficas y tablas
- **CSV**: Exportación de datos para análisis en Excel

#### Contenido del reporte:
- Total de ventas en el período
- Cantidad de transacciones
- Ticket promedio
- Productos más vendidos
- Clientes frecuentes
- Gráficas de tendencias

### 7. Dashboard con Gráficas de Ventas

Implementar página principal (Home) con visualización de datos:

#### Gráficas a considerar:
- Ventas del día/semana/mes (gráfica de líneas)
- Productos más vendidos (gráfica de barras)
- Ventas por categoría (gráfica de pastel)
- Comparativa de ventas mes actual vs mes anterior
- Clientes más frecuentes
- Tendencias de ventas por hora del día

**Tecnologías sugeridas**: Chart.js, Recharts, ApexCharts

### 8. Notificación de Stock Bajo

Implementar sistema de alertas automáticas:

- Monitorear stock de productos en tiempo real
- Cuando un producto llegue a **10 unidades o menos**, enviar notificación
- Notificación por **correo electrónico** a usuarios con rol de **Gerente**
- El correo debe incluir:
  - Nombre del producto
  - Cantidad actual en stock
  - Código de barras
  - Sugerencia de reorden

#### Entidades necesarias:
- **EmailLog**: Registrar todos los correos enviados (fecha, destinatario, asunto, estado, errores)

**Tecnologías sugeridas**: SMTP, SendGrid, Mailgun

### 9. Autenticación con JWT

Implementar sistema de autenticación completo:

#### Tokens:
- **Access Token**: Token de corta duración (15-30 minutos) para acceder a recursos
- **Refresh Token**: Token de larga duración (7-30 días) para renovar el access token

#### Endpoints necesarios:
- `POST /auth/login` - Autenticación de usuario
- `POST /auth/refresh` - Renovar access token usando refresh token
- `POST /auth/logout` - Invalidar tokens

#### Seguridad:
- Encriptación de contraseñas (BCrypt o similar)
- Almacenamiento seguro de refresh tokens
- Expiración y renovación automática de tokens
- Revocación de tokens al cambiar contraseña

#### Entidades necesarias:
- **RefreshToken**: Almacenar tokens de refresco en BD (UserId, Token, ExpiresAt, CreatedAt, RevokedAt, IsRevoked)

### 10. Control de Acceso Basado en Roles (RBAC)

Implementar autorización usando roles:

#### Roles definidos:
- **Administrador**: Acceso total al sistema
- **Gerente**: Acceso a reportes, ventas, inventario, usuarios
- **Vendedor**: Acceso solo a ventas y consulta de productos

#### Restricciones por rol:
- Administrador: Puede hacer todo
- Gerente: No puede eliminar usuarios ni modificar roles
- Vendedor: Solo puede crear ventas y consultar productos/clientes

**Implementación**: Usar `[Authorize(Roles = "...")]` en controllers

### 11. Recuperación de Contraseña

Implementar sistema de recuperación de contraseña:

#### Flujo:
1. Usuario solicita recuperación ingresando su email
2. Sistema genera código de verificación (6 dígitos)
3. Código se envía por:
   - **Email** al correo registrado
   - **WhatsApp** al número registrado (opcional)
4. Usuario ingresa código para validar
5. Si el código es válido, permite establecer nueva contraseña

#### Seguridad:
- Código expira en 15 minutos
- Máximo 3 intentos de validación
- Códigos de un solo uso
- Notificación de cambio de contraseña exitoso

#### Entidades necesarias:
- **PasswordResetToken**: Almacenar códigos de recuperación (UserId, Code, ExpiresAt, CreatedAt, AttemptCount, IsUsed)

**Tecnologías sugeridas**:
- Email: SMTP, SendGrid
- WhatsApp: Twilio API

### 12. Chat en Tiempo Real con WebSockets

Implementar sistema de mensajería interna:

#### Funcionalidad:
- Chat en tiempo real usando **WebSockets**
- Comunicación permitida entre:
  - **Gerente/Administrador ↔ Vendedor**
- Comunicación **NO permitida** entre:
  - **Vendedor ↔ Vendedor**

#### Características:
- Mensajes en tiempo real
- Historial de conversaciones
- Indicadores de mensaje leído/no leído
- Notificaciones de nuevos mensajes
- Búsqueda de conversaciones

#### Entidades necesarias:
- **ChatMessage**: Mensajes individuales (ConversationId, SenderId, Message, SentAt, IsRead)
- **Conversation**: Agrupación de mensajes entre dos usuarios (User1Id, User2Id, CreatedAt, LastMessageAt)

**Tecnologías sugeridas**: SignalR (.NET)

---

## Recomendaciones Adicionales

### 1. **Auditoría de Operaciones**
- **Qué es**: Registrar todas las operaciones importantes (quién, qué, cuándo)
- **Para qué sirve**: Trazabilidad, seguridad, compliance, debugging
- **Implementación**: Tabla de auditoría con trigger/interceptor de EF Core

### 2. **Soft Delete Global**
- **Qué es**: No eliminar físicamente registros, solo marcarlos como eliminados
- **Para qué sirve**: Recuperación de datos, auditoría, cumplimiento legal
- **Implementación**: Campo `DeletedAt` en todas las entidades

### 3. **Paginación en Listados**
- **Qué es**: Devolver datos en páginas en lugar de todo de golpe
- **Para qué sirve**: Performance, mejor UX, reducir carga del servidor
- **Implementación**: `PagedResult<T>` con `PageNumber`, `PageSize`, `TotalCount`

### 4. **Rate Limiting**
- **Qué es**: Limitar cantidad de peticiones por usuario/IP en un tiempo
- **Para qué sirve**: Prevenir abuso, ataques DDoS, controlar uso de API
- **Implementación**: Middleware de ASP.NET Core Rate Limiting

### 5. **Caché de Consultas Frecuentes**
- **Qué es**: Almacenar resultados de queries frecuentes en memoria
- **Para qué sirve**: Mejorar performance, reducir carga en BD
- **Implementación**: Redis, MemoryCache de .NET

### 6. **Versionado de API**
- **Qué es**: Mantener múltiples versiones de la API simultáneamente
- **Para qué sirve**: Evolución sin romper clientes existentes
- **Implementación**: `api/v1/products`, `api/v2/products`

### 7. **Health Checks**
- **Qué es**: Endpoints que reportan estado del sistema y dependencias
- **Para qué sirve**: Monitoreo, alertas, integración con orquestadores
- **Implementación**: ASP.NET Core Health Checks

### 8. **Logging Estructurado**
- **Qué es**: Logs en formato estructurado (JSON) en lugar de texto plano
- **Para qué sirve**: Búsqueda eficiente, análisis, correlación de eventos
- **Implementación**: Serilog con sinks (Console, File, Seq, ELK)

### 9. **Background Jobs**
- **Qué es**: Tareas que se ejecutan en segundo plano
- **Para qué sirve**: Envío de emails, generación de reportes pesados, limpieza de datos
- **Implementación**: Hangfire, Quartz.NET

### 10. **Feature Flags**
- **Qué es**: Activar/desactivar funcionalidades sin deployar código
- **Para qué sirve**: A/B testing, rollout gradual, toggle de emergencia
- **Implementación**: LaunchDarkly, Azure App Configuration

### 11. **API Documentation Interactiva**
- **Qué es**: Documentación auto-generada de la API con UI para probar
- **Para qué sirve**: Facilitar integración, testing manual, onboarding
- **Implementación**: Swagger/OpenAPI con Swagger UI

### 12. **Validación de Datos con FluentValidation**
- **Qué es**: Librería para validaciones complejas y legibles
- **Para qué sirve**: Separar validaciones del dominio, reutilización, testing
- **Implementación**: FluentValidation con integration en MediatR pipeline

### 13. **Manejo de Concurrencia Optimista**
- **Qué es**: Detectar conflictos cuando múltiples usuarios modifican lo mismo
- **Para qué sirve**: Evitar sobrescribir cambios de otros usuarios
- **Implementación**: RowVersion/Timestamp en EF Core

### 14. **Respaldo Automático de Base de Datos**
- **Qué es**: Backup programado de la BD
- **Para qué sirve**: Recuperación ante desastres, migraciones seguras
- **Implementación**: SQL Server Agent, scripts programados, Azure Backup

### 15. **Multi-tenancy** (Opcional para futuro)
- **Qué es**: Un sistema que sirve a múltiples negocios/tiendas
- **Para qué sirve**: SaaS, escalar el producto a múltiples clientes
- **Implementación**: TenantId en entidades, filtros globales EF Core

### 16. **Integración con Pasarelas de Pago**
- **Qué es**: Procesar pagos con tarjeta electrónicamente
- **Para qué sirve**: Modernizar el POS, aceptar más métodos de pago
- **Implementación**: Stripe, PayPal, Mercado Pago

### 17. **Código QR para Productos**
- **Qué es**: Generar QR codes para productos
- **Para qué sirve**: Facilitar búsqueda rápida, etiquetado moderno
- **Implementación**: QRCoder, ZXing.NET

### 18. **Descuentos y Promociones**
- **Qué es**: Sistema para aplicar descuentos por producto, categoría, o cliente
- **Para qué sirve**: Aumentar ventas, fidelización de clientes
- **Implementación**: Entidades Promotion, DiscountRule con Strategy Pattern

### 19. **Inventario Multi-almacén**
- **Qué es**: Gestionar stock en múltiples ubicaciones físicas
- **Para qué sirve**: Negocios con varias sucursales o bodegas
- **Implementación**: Entidad Warehouse, relación Product-Warehouse-Stock

### 20. **Sincronización Offline**
- **Qué es**: Permitir ventas sin internet y sincronizar después
- **Para qué sirve**: Continuidad del negocio ante fallas de conexión
- **Implementación**: IndexedDB en frontend, API de sincronización

---

## Tecnologías y Herramientas Recomendadas

### Backend
- **.NET 10** (C# 13)
- **Entity Framework Core 10**
- **SQL Server**
- **JWT** para autenticación
- **SignalR** para WebSockets
- **Serilog** para logging
- **MediatR** para CQRS (o implementación custom)
- **FluentValidation** para validaciones
- **Mapster** para mapeo de objetos

### Generación de Documentos
- **QuestPDF** para generar PDFs
- **ClosedXML** para generar Excel/CSV

### Notificaciones
- **MailKit** para emails
- **Twilio** para SMS/WhatsApp

### Testing
- **xUnit** para unit tests
- **Moq** para mocking
- **FluentAssertions** para assertions expresivas
- **TestContainers** para integration tests con BD real

### Frontend (si aplica)
- **React** o **Angular** o **Blazor**
- **Chart.js** o **Recharts** para gráficas
- **Axios** para llamadas HTTP
- **SignalR Client** para WebSockets

---

## Notas Finales

Este proyecto está diseñado como una **experiencia de aprendizaje completa** que cubre:

- ✅ Arquitectura limpia y principios SOLID
- ✅ Patrones de diseño (Repository, Unit of Work, CQRS, Specification)
- ✅ Domain-Driven Design
- ✅ Seguridad (autenticación, autorización, encriptación)
- ✅ Comunicación en tiempo real (WebSockets)
- ✅ Generación de documentos (PDF, CSV)
- ✅ Envío de notificaciones (Email, SMS)
- ✅ Reportes y análisis de datos
- ✅ Testing y calidad de código

**Recuerda**: La clave del aprendizaje es implementar paso a paso, entender cada concepto antes de avanzar, y no tener miedo de experimentar y cometer errores.

---

**Versión**: 1.0
**Última actualización**: 2026-02-08
**Autor**: Plan generado para aprendizaje del proyecto SuperPOS
