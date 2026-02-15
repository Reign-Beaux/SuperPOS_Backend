using Application.UseCases.Dashboard.DTOs;

namespace Application.UseCases.Reports.DTOs;

/// <summary>
/// Filtros avanzados para generación de reportes.
/// </summary>
/// <param name="Period">Período predefinido (Hoy, Esta Semana, Este Mes, etc.).</param>
/// <param name="CustomStartDate">Fecha inicio para período personalizado.</param>
/// <param name="CustomEndDate">Fecha fin para período personalizado.</param>
/// <param name="CustomerId">Filtrar por cliente específico (opcional).</param>
/// <param name="ProductId">Filtrar por producto específico (opcional).</param>
/// <param name="DaysOfWeek">Filtrar por días de la semana específicos (ej: [Lunes, Viernes]).</param>
/// <param name="SpecificMonth">Filtrar por mes específico (1-12).</param>
/// <param name="SpecificYear">Filtrar por año específico (ej: 2024).</param>
/// <param name="TopItemsLimit">Límite de elementos en rankings (Top N productos/clientes).</param>
/// <param name="Format">Formato de exportación (PDF o Excel).</param>
public record ReportFilterDTO(
    DashboardPeriod Period = DashboardPeriod.Today,
    DateTime? CustomStartDate = null,
    DateTime? CustomEndDate = null,
    Guid? CustomerId = null,
    Guid? ProductId = null,
    List<DayOfWeek>? DaysOfWeek = null,
    int? SpecificMonth = null,
    int? SpecificYear = null,
    int TopItemsLimit = 10,
    ExportFormat Format = ExportFormat.Pdf
);
