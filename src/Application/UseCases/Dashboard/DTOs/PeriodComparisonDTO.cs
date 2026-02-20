namespace Application.UseCases.Dashboard.DTOs;

/// <summary>
/// Comparación entre el período actual y el período anterior.
/// Incluye porcentajes de cambio en métricas clave.
/// </summary>
public record PeriodComparisonDTO(
    string PeriodName,                    // Nombre descriptivo del período (ej: "Este mes vs Mes anterior")
    DailySummaryDTO CurrentPeriod,        // Resumen del período actual
    DailySummaryDTO PreviousPeriod,       // Resumen del período anterior
    decimal RevenueChangePercent,         // Cambio porcentual en ingresos
    decimal SalesChangePercent,           // Cambio porcentual en número de ventas
    decimal AverageTicketChangePercent,   // Cambio porcentual en tamaño promedio del ticket
    decimal ItemsSoldChangePercent,       // Cambio porcentual en artículos vendidos
    decimal CustomersChangePercent        // Cambio porcentual en clientes únicos
);
