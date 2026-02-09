namespace Application.Interfaces.Services;

/// <summary>
/// Service for generating PDF documents (tickets, reports, etc).
/// </summary>
public interface ITicketService
{
    /// <summary>
    /// Generates a PDF ticket for a sale.
    /// </summary>
    /// <param name="saleId">The ID of the sale</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>PDF bytes</returns>
    Task<byte[]> GenerateSaleTicketAsync(Guid saleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates and saves a PDF ticket to a file.
    /// </summary>
    /// <param name="saleId">The ID of the sale</param>
    /// <param name="outputPath">The output file path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The path to the saved file</returns>
    Task<string> GenerateAndSaveTicketAsync(Guid saleId, string outputPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a PDF report for a cash register closing.
    /// </summary>
    /// <param name="cashRegisterId">The ID of the cash register</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>PDF bytes</returns>
    Task<byte[]> GenerateCashRegisterReportAsync(Guid cashRegisterId, CancellationToken cancellationToken = default);
}
