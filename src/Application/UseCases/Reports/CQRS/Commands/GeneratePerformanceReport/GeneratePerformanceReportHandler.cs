using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Services;
using Application.UseCases.Dashboard.CQRS.Queries.GetDailySummary;
using Application.UseCases.Dashboard.CQRS.Queries.GetPeriodComparison;
using Application.UseCases.Dashboard.CQRS.Queries.GetTopCustomers;
using Application.UseCases.Dashboard.CQRS.Queries.GetTopProducts;
using Application.UseCases.Dashboard.DTOs;
using Application.UseCases.Reports.DTOs;

namespace Application.UseCases.Reports.CQRS.Commands.GeneratePerformanceReport;

public class GeneratePerformanceReportHandler(
    IMediator mediator,
    IReportService reportService)
    : IRequestHandler<GeneratePerformanceReportCommand, OperationResult<FileResultDTO>>
{
    public async Task<OperationResult<FileResultDTO>> Handle(
        GeneratePerformanceReportCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validar filtros
        var filters = request.Filters;
        if (filters.Period == DashboardPeriod.Custom)
        {
            if (!filters.CustomStartDate.HasValue || !filters.CustomEndDate.HasValue)
                return Result.Error(ErrorResult.BadRequest,
                    "El período personalizado requiere fecha de inicio y fin.");
        }

        // 2. Llamar a las queries del Dashboard (REUTILIZACIÓN)
        var summaryResult = await mediator.Send(
            new GetDailySummaryQuery(filters.Period, filters.CustomStartDate, filters.CustomEndDate),
            cancellationToken);

        if (!summaryResult.IsSuccess)
            return Result.Error(ErrorResult.InternalServerError,
                "Error al obtener el resumen de ventas.");

        var topProductsResult = await mediator.Send(
            new GetTopProductsQuery(filters.Period, filters.CustomStartDate, filters.CustomEndDate, filters.TopItemsLimit),
            cancellationToken);

        if (!topProductsResult.IsSuccess)
            return Result.Error(ErrorResult.InternalServerError,
                "Error al obtener los productos más vendidos.");

        var topCustomersResult = await mediator.Send(
            new GetTopCustomersQuery(filters.Period, filters.CustomStartDate, filters.CustomEndDate, filters.TopItemsLimit),
            cancellationToken);

        if (!topCustomersResult.IsSuccess)
            return Result.Error(ErrorResult.InternalServerError,
                "Error al obtener los clientes más frecuentes.");

        var comparisonResult = await mediator.Send(
            new GetPeriodComparisonQuery(filters.Period),
            cancellationToken);

        // 3. Construir el título del reporte
        var reportTitle = $"Reporte de Rendimiento - {DashboardPeriodHelper.GetPeriodName(filters.Period)}";
        if (filters.Period == DashboardPeriod.Custom && filters.CustomStartDate.HasValue && filters.CustomEndDate.HasValue)
        {
            reportTitle = $"Reporte de Rendimiento - {filters.CustomStartDate.Value:dd/MM/yyyy} a {filters.CustomEndDate.Value:dd/MM/yyyy}";
        }

        // 4. Construir el DTO del reporte
        var reportData = new PerformanceReportDTO(
            ReportTitle: reportTitle,
            GeneratedAt: DateTime.UtcNow,
            Filters: filters,
            Summary: summaryResult.Value,
            Comparison: comparisonResult.IsSuccess ? comparisonResult.Value : null,
            TopProducts: topProductsResult.Value,
            TopCustomers: topCustomersResult.Value
        );

        // 5. Generar el archivo (PDF o Excel)
        byte[] fileBytes;
        string fileName;
        string contentType;

        if (filters.Format == ExportFormat.Pdf)
        {
            fileBytes = await reportService.GeneratePerformanceReportPdfAsync(reportData, cancellationToken);
            fileName = $"Reporte_Rendimiento_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
            contentType = "application/pdf";
        }
        else
        {
            fileBytes = await reportService.GeneratePerformanceReportExcelAsync(reportData, cancellationToken);
            fileName = $"Reporte_Rendimiento_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        }

        return Result.Success(new FileResultDTO(fileBytes, fileName, contentType));
    }
}
