namespace Application.UseCases.Reports.DTOs;

/// <summary>
/// Resultado de generación de archivo para descarga.
/// </summary>
/// <param name="FileBytes">Contenido del archivo en bytes.</param>
/// <param name="FileName">Nombre del archivo con extensión.</param>
/// <param name="ContentType">Tipo MIME del archivo.</param>
public record FileResultDTO(
    byte[] FileBytes,
    string FileName,
    string ContentType
);
