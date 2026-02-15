using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Reports.DTOs;

namespace Application.UseCases.Reports.CQRS.Commands.GeneratePerformanceReport;

/// <summary>
/// Comando para generar un reporte de rendimiento (combinado).
/// </summary>
/// <param name="Filters">Filtros para el reporte (período, top items, formato).</param>
public record GeneratePerformanceReportCommand(
    ReportFilterDTO Filters
) : IRequest<OperationResult<FileResultDTO>>;
