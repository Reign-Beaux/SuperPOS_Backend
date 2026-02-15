using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Services;
using Application.UseCases.Dashboard.CQRS.Queries.GetDailySummary;
using Application.UseCases.Dashboard.CQRS.Queries.GetHourlyTrends;
using Application.UseCases.Dashboard.CQRS.Queries.GetPeriodComparison;
using Application.UseCases.Dashboard.CQRS.Queries.GetTopCustomers;
using Application.UseCases.Dashboard.CQRS.Queries.GetTopProducts;
using Application.UseCases.Dashboard.DTOs;
using Application.UseCases.Reports.DTOs;
using Domain.Specifications.Sales;

namespace Application.UseCases.Reports.CQRS.Commands.GenerateSalesReport;

public class GenerateSalesReportHandler(
    IMediator mediator,
    IUnitOfWork unitOfWork,
    IReportService reportService)
    : IRequestHandler<GenerateSalesReportCommand, OperationResult<FileResultDTO>>
{
    public async Task<OperationResult<FileResultDTO>> Handle(
        GenerateSalesReportCommand request,
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

        // Validar mes específico
        if (filters.SpecificMonth.HasValue && (filters.SpecificMonth < 1 || filters.SpecificMonth > 12))
            return Result.Error(ErrorResult.BadRequest,
                "El mes debe estar entre 1 y 12.");

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

        var hourlyResult = await mediator.Send(
            new GetHourlyTrendsQuery(filters.Period, filters.CustomStartDate, filters.CustomEndDate),
            cancellationToken);

        if (!hourlyResult.IsSuccess)
            return Result.Error(ErrorResult.InternalServerError,
                "Error al obtener las tendencias horarias.");

        var comparisonResult = await mediator.Send(
            new GetPeriodComparisonQuery(filters.Period),
            cancellationToken);

        // 3. Aplicar filtros avanzados y obtener ventas detalladas (si se requiere)
        List<SaleItemDTO>? detailedSales = null;
        if (request.IncludeDetailedSales)
        {
            var (start, end) = DashboardPeriodHelper.GetDateRange(
                filters.Period, filters.CustomStartDate, filters.CustomEndDate);

            var specification = new SalesByDateRangeSpecification(start, end);
            var sales = await unitOfWork.Sales.ListAsync(specification, cancellationToken);

            // Aplicar filtros avanzados
            var filteredSales = sales.AsEnumerable();

            if (filters.CustomerId.HasValue)
                filteredSales = filteredSales.Where(s => s.CustomerId == filters.CustomerId.Value);

            if (filters.ProductId.HasValue)
                filteredSales = filteredSales.Where(s =>
                    s.SaleDetails.Any(sd => sd.ProductId == filters.ProductId.Value));

            if (filters.DaysOfWeek?.Any() == true)
                filteredSales = filteredSales.Where(s =>
                    filters.DaysOfWeek.Contains(s.CreatedAt.DayOfWeek));

            if (filters.SpecificMonth.HasValue)
                filteredSales = filteredSales.Where(s =>
                    s.CreatedAt.Month == filters.SpecificMonth.Value);

            if (filters.SpecificYear.HasValue)
                filteredSales = filteredSales.Where(s =>
                    s.CreatedAt.Year == filters.SpecificYear.Value);

            // Mapear a DTOs
            detailedSales = filteredSales.Select(s =>
            {
                // Calcular subtotal e IVA (16% incluido en total)
                var subtotal = s.TotalAmount / 1.16m;
                var tax = s.TotalAmount - subtotal;

                return new SaleItemDTO(
                    SaleId: s.Id,
                    Date: s.CreatedAt,
                    CustomerName: s.Customer?.Name ?? "N/A",
                    UserName: s.User?.Name ?? "N/A",
                    ProductsCount: s.SaleDetails.Count,
                    TotalItems: s.SaleDetails.Sum(sd => sd.Quantity),
                    Subtotal: subtotal,
                    Tax: tax,
                    Total: s.TotalAmount
                );
            }).ToList();
        }

        // 4. Construir el título del reporte
        var reportTitle = $"Reporte de Ventas - {DashboardPeriodHelper.GetPeriodName(filters.Period)}";
        if (filters.Period == DashboardPeriod.Custom && filters.CustomStartDate.HasValue && filters.CustomEndDate.HasValue)
        {
            reportTitle = $"Reporte de Ventas - {filters.CustomStartDate.Value:dd/MM/yyyy} a {filters.CustomEndDate.Value:dd/MM/yyyy}";
        }

        // 5. Construir el DTO del reporte
        var reportData = new SalesReportDTO(
            ReportTitle: reportTitle,
            GeneratedAt: DateTime.UtcNow,
            Filters: filters,
            Summary: summaryResult.Value,
            TopProducts: topProductsResult.Value,
            TopCustomers: topCustomersResult.Value,
            HourlyTrends: hourlyResult.Value,
            Comparison: comparisonResult.IsSuccess ? comparisonResult.Value : null,
            DetailedSales: detailedSales
        );

        // 6. Generar el archivo (PDF o Excel)
        byte[] fileBytes;
        string fileName;
        string contentType;

        if (filters.Format == ExportFormat.Pdf)
        {
            fileBytes = await reportService.GenerateSalesReportPdfAsync(reportData, cancellationToken);
            fileName = $"Reporte_Ventas_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
            contentType = "application/pdf";
        }
        else
        {
            fileBytes = await reportService.GenerateSalesReportExcelAsync(reportData, cancellationToken);
            fileName = $"Reporte_Ventas_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        }

        return Result.Success(new FileResultDTO(fileBytes, fileName, contentType));
    }
}
