using Application.UseCases.Dashboard.DTOs;

namespace Application.UseCases.Reports.DTOs;

/// <summary>
/// Datos completos del reporte de ventas.
/// </summary>
/// <param name="ReportTitle">Título del reporte (ej: "Reporte de Ventas - Febrero 2024").</param>
/// <param name="GeneratedAt">Fecha y hora de generación del reporte.</param>
/// <param name="Filters">Filtros aplicados al reporte.</param>
/// <param name="Summary">Resumen de métricas diarias.</param>
/// <param name="TopProducts">Top productos más vendidos.</param>
/// <param name="TopCustomers">Top clientes con mayor gasto.</param>
/// <param name="HourlyTrends">Tendencias de ventas por hora.</param>
/// <param name="Comparison">Comparación con período anterior (opcional).</param>
/// <param name="DetailedSales">Lista detallada de ventas (solo para Excel).</param>
public record SalesReportDTO(
    string ReportTitle,
    DateTime GeneratedAt,
    ReportFilterDTO Filters,
    DailySummaryDTO Summary,
    List<TopProductDTO> TopProducts,
    List<TopCustomerDTO> TopCustomers,
    List<HourlySaleDTO> HourlyTrends,
    PeriodComparisonDTO? Comparison,
    List<SaleItemDTO>? DetailedSales
);
