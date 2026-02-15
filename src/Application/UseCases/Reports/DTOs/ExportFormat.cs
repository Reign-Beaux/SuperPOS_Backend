namespace Application.UseCases.Reports.DTOs;

/// <summary>
/// Formato de exportación para reportes.
/// </summary>
public enum ExportFormat
{
    /// <summary>
    /// Exportar como PDF.
    /// </summary>
    Pdf = 0,

    /// <summary>
    /// Exportar como Excel (XLSX).
    /// </summary>
    Excel = 1
}
