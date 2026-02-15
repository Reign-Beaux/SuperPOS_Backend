using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Reports.DTOs;

namespace Application.UseCases.Reports.CQRS.Commands.GenerateInventoryReport;

/// <summary>
/// Comando para generar un reporte de inventario.
/// </summary>
/// <param name="Format">Formato de exportación (PDF o Excel).</param>
/// <param name="IncludeAllItems">Incluir todos los productos en el reporte (solo para Excel).</param>
public record GenerateInventoryReportCommand(
    ExportFormat Format = ExportFormat.Pdf,
    bool IncludeAllItems = false
) : IRequest<OperationResult<FileResultDTO>>;
