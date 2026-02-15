using Application.UseCases.Dashboard.DTOs;

namespace Application.UseCases.Reports.DTOs;

/// <summary>
/// Datos completos del reporte de rendimiento (combinado).
/// </summary>
/// <param name="ReportTitle">Título del reporte.</param>
/// <param name="GeneratedAt">Fecha y hora de generación.</param>
/// <param name="Filters">Filtros aplicados al reporte.</param>
/// <param name="Summary">Resumen de métricas del período.</param>
/// <param name="Comparison">Comparación con período anterior.</param>
/// <param name="TopProducts">Top productos más vendidos.</param>
/// <param name="TopCustomers">Top clientes con mayor gasto.</param>
public record PerformanceReportDTO(
    string ReportTitle,
    DateTime GeneratedAt,
    ReportFilterDTO Filters,
    DailySummaryDTO Summary,
    PeriodComparisonDTO? Comparison,
    List<TopProductDTO> TopProducts,
    List<TopCustomerDTO> TopCustomers
);
