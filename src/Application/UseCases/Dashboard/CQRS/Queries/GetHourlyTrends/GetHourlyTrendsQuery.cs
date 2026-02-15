using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Dashboard.DTOs;

namespace Application.UseCases.Dashboard.CQRS.Queries.GetHourlyTrends;

/// <summary>
/// Query para obtener tendencias de ventas por hora del día.
/// Muestra la distribución de ventas en las 24 horas (0-23).
/// </summary>
public record GetHourlyTrendsQuery(
    DashboardPeriod Period = DashboardPeriod.Today,
    DateTime? CustomStartDate = null,
    DateTime? CustomEndDate = null
) : IRequest<OperationResult<List<HourlySaleDTO>>>;
