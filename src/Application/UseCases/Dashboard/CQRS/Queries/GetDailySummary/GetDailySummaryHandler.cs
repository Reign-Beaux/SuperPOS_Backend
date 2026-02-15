using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Dashboard.DTOs;
using Domain.Entities.Dashboard;
using Domain.Specifications.Sales;

namespace Application.UseCases.Dashboard.CQRS.Queries.GetDailySummary;

public class GetDailySummaryHandler
    : IRequestHandler<GetDailySummaryQuery, OperationResult<DailySummaryDTO>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDailySummaryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<DailySummaryDTO>> Handle(
        GetDailySummaryQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Calcular el rango de fechas
        var (startDate, endDate) = DashboardPeriodHelper.GetDateRange(
            request.Period,
            request.CustomStartDate,
            request.CustomEndDate);

        // 2. Obtener estadísticas de resumen desde el repositorio
        var summary = await _unitOfWork.Sales.GetSalesSummaryAsync(
            startDate,
            endDate,
            cancellationToken);

        // 3. Obtener ventas para cálculos de min/max
        var specification = new SalesByDateRangeSpecification(startDate, endDate);
        var sales = await _unitOfWork.Sales.ListAsync(specification, cancellationToken);

        // 4. Calcular clientes únicos
        var uniqueCustomers = sales.Select(s => s.CustomerId).Distinct().Count();

        // 5. Calcular ventas más alta/baja (manejar caso vacío)
        var highestSale = sales.Any() ? sales.Max(s => s.TotalAmount) : 0;
        var lowestSale = sales.Any() ? sales.Min(s => s.TotalAmount) : 0;

        // 6. Crear el DTO de respuesta
        var dto = new DailySummaryDTO(
            StartDate: startDate,
            EndDate: endDate,
            TotalSales: summary.TotalSales,
            TotalRevenue: summary.TotalRevenue,
            AverageTicketSize: summary.AvgTicketSize,
            TotalItemsSold: summary.TotalItemsSold,
            TotalCustomers: uniqueCustomers,
            HighestSale: highestSale,
            LowestSale: lowestSale
        );

        return Result.Success(dto);
    }
}
