using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Dashboard.DTOs;

namespace Application.UseCases.Dashboard.CQRS.Queries.GetHourlyTrends;

public class GetHourlyTrendsHandler
    : IRequestHandler<GetHourlyTrendsQuery, OperationResult<List<HourlySaleDTO>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetHourlyTrendsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<List<HourlySaleDTO>>> Handle(
        GetHourlyTrendsQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Calcular el rango de fechas
        var (startDate, endDate) = DashboardPeriodHelper.GetDateRange(
            request.Period,
            request.CustomStartDate,
            request.CustomEndDate);

        // 2. Obtener datos horarios desde el repositorio
        var hourlyData = await _unitOfWork.Sales.GetHourlySalesAsync(
            startDate,
            endDate,
            cancellationToken);

        // 3. Mapear a DTOs con etiquetas formateadas
        var dtos = hourlyData.Select(h => new HourlySaleDTO(
            Hour: h.Hour,
            HourLabel: $"{h.Hour:D2}:00 - {(h.Hour + 1) % 24:D2}:00",
            SalesCount: h.SalesCount,
            TotalRevenue: h.TotalAmount
        )).ToList();

        return Result.Success(dtos);
    }
}
