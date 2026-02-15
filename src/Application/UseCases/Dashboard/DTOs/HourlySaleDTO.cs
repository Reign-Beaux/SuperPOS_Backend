namespace Application.UseCases.Dashboard.DTOs;

/// <summary>
/// DTO para tendencias de ventas por hora del día.
/// </summary>
public record HourlySaleDTO(
    int Hour,                  // Hora del día (0-23)
    string HourLabel,          // Etiqueta formateada (ej: "09:00 - 10:00")
    int SalesCount,            // Número de ventas en esta hora
    decimal TotalRevenue       // Ingresos totales en esta hora
);
