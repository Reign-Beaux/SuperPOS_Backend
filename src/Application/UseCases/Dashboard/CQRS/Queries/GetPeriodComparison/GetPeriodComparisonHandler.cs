using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Dashboard.CQRS.Queries.GetDailySummary;
using Application.UseCases.Dashboard.DTOs;
using Domain.Entities.Dashboard;

namespace Application.UseCases.Dashboard.CQRS.Queries.GetPeriodComparison;

public class GetPeriodComparisonHandler
    : IRequestHandler<GetPeriodComparisonQuery, OperationResult<PeriodComparisonDTO>>
{
    private readonly IMediator _mediator;

    public GetPeriodComparisonHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<OperationResult<PeriodComparisonDTO>> Handle(
        GetPeriodComparisonQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Validar que el período sea comparable (no Custom)
        if (request.Period == DashboardPeriod.Custom)
        {
            return Result.Error(
                ErrorResult.BadRequest,
                detail: DashboardMessages.ComparisonNotSupportedForCustom);
        }

        // 2. Calcular rangos de fechas para período actual y anterior
        var (currentStart, currentEnd) = DashboardPeriodHelper.GetDateRange(request.Period);
        var timeSpan = currentEnd - currentStart;
        var previousEnd = currentStart;
        var previousStart = previousEnd - timeSpan;

        // 3. Obtener resúmenes para ambos períodos en paralelo
        var currentQuery = new GetDailySummaryQuery(DashboardPeriod.Custom, currentStart, currentEnd);
        var previousQuery = new GetDailySummaryQuery(DashboardPeriod.Custom, previousStart, previousEnd);

        var currentTask = _mediator.Send(currentQuery, cancellationToken);
        var previousTask = _mediator.Send(previousQuery, cancellationToken);

        await Task.WhenAll(currentTask, previousTask);

        var currentResult = await currentTask;
        var previousResult = await previousTask;

        // 4. Verificar que ambas consultas fueron exitosas
        if (!currentResult.IsSuccess || !previousResult.IsSuccess)
        {
            return Result.Error(ErrorResult.InternalServerError, DashboardMessages.ComparisonFailed);
        }

        // 5. Calcular porcentajes de cambio
        var revenueChange = CalculatePercentChange(
            previousResult.Value!.TotalRevenue,
            currentResult.Value!.TotalRevenue);

        var salesCountChange = CalculatePercentChange(
            previousResult.Value.TotalSales,
            currentResult.Value.TotalSales);

        var avgTicketChange = CalculatePercentChange(
            previousResult.Value.AverageTicketSize,
            currentResult.Value.AverageTicketSize);

        // 6. Crear DTO de comparación
        var comparison = new PeriodComparisonDTO(
            PeriodName: GetPeriodComparisonName(request.Period),
            CurrentPeriod: currentResult.Value,
            PreviousPeriod: previousResult.Value,
            RevenueChangePercent: revenueChange,
            SalesCountChangePercent: salesCountChange,
            AvgTicketChangePercent: avgTicketChange
        );

        return Result.Success(comparison);
    }

    /// <summary>
    /// Calcula el porcentaje de cambio entre dos valores decimales.
    /// </summary>
    private static decimal CalculatePercentChange(decimal previous, decimal current)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return ((current - previous) / previous) * 100;
    }

    /// <summary>
    /// Calcula el porcentaje de cambio entre dos valores enteros.
    /// </summary>
    private static decimal CalculatePercentChange(int previous, int current)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return ((decimal)(current - previous) / previous) * 100;
    }

    /// <summary>
    /// Obtiene el nombre descriptivo de la comparación en español.
    /// </summary>
    private static string GetPeriodComparisonName(DashboardPeriod period)
    {
        return period switch
        {
            DashboardPeriod.Today => "Hoy vs Ayer",
            DashboardPeriod.Yesterday => "Ayer vs Anteayer",
            DashboardPeriod.ThisWeek => "Esta semana vs Semana anterior",
            DashboardPeriod.LastWeek => "Semana anterior vs Hace dos semanas",
            DashboardPeriod.ThisMonth => "Este mes vs Mes anterior",
            DashboardPeriod.LastMonth => "Mes anterior vs Hace dos meses",
            _ => "Período actual vs anterior"
        };
    }
}
