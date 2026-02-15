namespace Application.UseCases.Dashboard.DTOs;

/// <summary>
/// DTO para clientes frecuentes con métricas de compras.
/// </summary>
public record TopCustomerDTO(
    Guid CustomerId,           // ID del cliente
    string CustomerName,       // Nombre del cliente
    int PurchaseCount,         // Número de compras realizadas
    decimal TotalSpent,        // Total gastado en el período
    decimal AverageTicketSize, // Tamaño promedio de compra
    DateTime LastPurchaseDate  // Fecha de última compra
);
