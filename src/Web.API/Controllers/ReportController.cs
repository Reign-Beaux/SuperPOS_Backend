using Application.DesignPatterns.Mediators.Interfaces;
using Application.UseCases.Reports.CQRS.Commands.GenerateSalesReport;
using Application.UseCases.Reports.CQRS.Commands.GenerateInventoryReport;
using Application.UseCases.Reports.CQRS.Commands.GeneratePerformanceReport;
using Microsoft.AspNetCore.Authorization;

namespace Web.API.Controllers;

/// <summary>
/// Controlador para generación de reportes avanzados con exportación a PDF y Excel.
/// </summary>
[Route("api/[controller]")]
[Authorize(Policy = "ManagerOrAbove")]
public class ReportController : BaseController
{
    private readonly IMediator _mediator;

    public ReportController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Genera un reporte de ventas con filtros avanzados.
    /// Soporta exportación a PDF o Excel con métricas detalladas, comparación de períodos, top productos/clientes y tendencias horarias.
    /// </summary>
    /// <param name="command">Comando con filtros y opciones de exportación.</param>
    /// <returns>Archivo PDF o Excel con el reporte de ventas.</returns>
    /// <response code="200">Reporte generado exitosamente. Descarga el archivo.</response>
    /// <response code="400">Error de validación en los filtros (ej: período personalizado sin fechas).</response>
    /// <response code="500">Error interno al generar el reporte.</response>
    [HttpPost("sales")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateSalesReport([FromBody] GenerateSalesReportCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return HandleResult(result);

        if (result.Value == null)
            return StatusCode(StatusCodes.Status500InternalServerError, "Error al generar el reporte.");

        var fileResult = result.Value;
        return File(fileResult.FileBytes, fileResult.ContentType, fileResult.FileName);
    }

    /// <summary>
    /// Genera un reporte de inventario con resumen de stock.
    /// Incluye productos agotados, productos con stock bajo y valor total del inventario.
    /// </summary>
    /// <param name="command">Comando con opciones de exportación.</param>
    /// <returns>Archivo PDF o Excel con el reporte de inventario.</returns>
    /// <response code="200">Reporte generado exitosamente. Descarga el archivo.</response>
    /// <response code="500">Error interno al generar el reporte.</response>
    [HttpPost("inventory")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateInventoryReport([FromBody] GenerateInventoryReportCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return HandleResult(result);

        if (result.Value == null)
            return StatusCode(StatusCodes.Status500InternalServerError, "Error al generar el reporte.");

        var fileResult = result.Value;
        return File(fileResult.FileBytes, fileResult.ContentType, fileResult.FileName);
    }

    /// <summary>
    /// Genera un reporte de rendimiento combinado (ventas + top productos + top clientes).
    /// Vista consolidada del desempeño del negocio en un período específico.
    /// </summary>
    /// <param name="command">Comando con filtros y opciones de exportación.</param>
    /// <returns>Archivo PDF o Excel con el reporte de rendimiento.</returns>
    /// <response code="200">Reporte generado exitosamente. Descarga el archivo.</response>
    /// <response code="400">Error de validación en los filtros.</response>
    /// <response code="500">Error interno al generar el reporte.</response>
    [HttpPost("performance")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GeneratePerformanceReport([FromBody] GeneratePerformanceReportCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return HandleResult(result);

        if (result.Value == null)
            return StatusCode(StatusCodes.Status500InternalServerError, "Error al generar el reporte.");

        var fileResult = result.Value;
        return File(fileResult.FileBytes, fileResult.ContentType, fileResult.FileName);
    }
}
