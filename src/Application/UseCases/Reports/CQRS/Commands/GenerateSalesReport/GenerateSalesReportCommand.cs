using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Reports.DTOs;

namespace Application.UseCases.Reports.CQRS.Commands.GenerateSalesReport;

/// <summary>
/// Comando para generar un reporte de ventas con filtros avanzados.
/// </summary>
/// <param name="Filters">Filtros para el reporte (período, cliente, días de semana, etc.).</param>
/// <param name="IncludeDetailedSales">Incluir lista detallada de ventas (solo para Excel).</param>
public record GenerateSalesReportCommand(
    ReportFilterDTO Filters,
    bool IncludeDetailedSales = false
) : IRequest<OperationResult<FileResultDTO>>;
