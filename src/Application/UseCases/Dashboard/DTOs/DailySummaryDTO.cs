namespace Application.UseCases.Dashboard.DTOs;

/// <summary>
/// Resumen estadístico de ventas para un período específico.
/// Incluye métricas agregadas de ventas, ingresos y clientes.
/// </summary>
public record DailySummaryDTO(
    DateTime StartDate,           // Fecha de inicio del período (UTC)
    DateTime EndDate,              // Fecha de fin del período (UTC)
    int TotalSales,                // Número total de ventas (transacciones)
    decimal TotalRevenue,          // Ingresos totales (suma de TotalAmount)
    decimal AverageTicketSize,     // Tamaño promedio del ticket (TotalRevenue / TotalSales)
    int TotalItemsSold,            // Cantidad total de artículos vendidos
    int TotalCustomers,            // Número de clientes únicos con compras
    decimal HighestSale,           // Venta más alta (TotalAmount)
    decimal LowestSale             // Venta más baja (TotalAmount)
);
