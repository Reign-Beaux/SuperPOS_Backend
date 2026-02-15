namespace Application.UseCases.Dashboard.DTOs;

/// <summary>
/// DTO para productos más vendidos con métricas de rendimiento.
/// </summary>
public record TopProductDTO(
    Guid ProductId,            // ID del producto
    string ProductName,        // Nombre del producto
    int QuantitySold,          // Cantidad total vendida
    decimal TotalRevenue,      // Ingresos totales generados por este producto
    int TransactionCount       // Número de ventas que incluyen este producto
);
