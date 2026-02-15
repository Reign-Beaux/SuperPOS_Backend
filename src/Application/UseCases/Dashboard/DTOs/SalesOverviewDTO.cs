namespace Application.UseCases.Dashboard.DTOs;

/// <summary>
/// Vista general completa del dashboard combinando múltiples analíticas.
/// Proporciona un snapshot integral del rendimiento de ventas.
/// </summary>
public record SalesOverviewDTO(
    DailySummaryDTO TodaySummary,          // Resumen del día actual
    DailySummaryDTO ThisWeekSummary,       // Resumen de la semana actual
    DailySummaryDTO ThisMonthSummary,      // Resumen del mes actual
    List<TopProductDTO> TopProducts,       // Top 5 productos más vendidos (mes actual)
    List<TopCustomerDTO> TopCustomers,     // Top 5 clientes frecuentes (mes actual)
    List<HourlySaleDTO> HourlyTrends       // Tendencias por hora (día actual)
);
