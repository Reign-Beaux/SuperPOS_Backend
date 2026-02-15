using Application.UseCases.Reports.DTOs;

namespace Application.Interfaces.Services;

/// <summary>
/// Servicio para generación de reportes en diferentes formatos (PDF, Excel).
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Genera un reporte de ventas en formato PDF.
    /// </summary>
    /// <param name="reportData">Datos del reporte de ventas.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Bytes del archivo PDF generado.</returns>
    Task<byte[]> GenerateSalesReportPdfAsync(SalesReportDTO reportData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera un reporte de ventas en formato Excel.
    /// </summary>
    /// <param name="reportData">Datos del reporte de ventas.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Bytes del archivo Excel generado.</returns>
    Task<byte[]> GenerateSalesReportExcelAsync(SalesReportDTO reportData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera un reporte de inventario en formato PDF.
    /// </summary>
    /// <param name="reportData">Datos del reporte de inventario.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Bytes del archivo PDF generado.</returns>
    Task<byte[]> GenerateInventoryReportPdfAsync(InventoryReportDTO reportData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera un reporte de inventario en formato Excel.
    /// </summary>
    /// <param name="reportData">Datos del reporte de inventario.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Bytes del archivo Excel generado.</returns>
    Task<byte[]> GenerateInventoryReportExcelAsync(InventoryReportDTO reportData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera un reporte de rendimiento en formato PDF.
    /// </summary>
    /// <param name="reportData">Datos del reporte de rendimiento.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Bytes del archivo PDF generado.</returns>
    Task<byte[]> GeneratePerformanceReportPdfAsync(PerformanceReportDTO reportData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera un reporte de rendimiento en formato Excel.
    /// </summary>
    /// <param name="reportData">Datos del reporte de rendimiento.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Bytes del archivo Excel generado.</returns>
    Task<byte[]> GeneratePerformanceReportExcelAsync(PerformanceReportDTO reportData, CancellationToken cancellationToken = default);
}
