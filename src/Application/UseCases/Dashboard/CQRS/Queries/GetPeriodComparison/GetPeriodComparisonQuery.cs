using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Dashboard.DTOs;

namespace Application.UseCases.Dashboard.CQRS.Queries.GetPeriodComparison;

/// <summary>
/// Query para comparar el período actual con el período anterior.
/// Calcula porcentajes de cambio en métricas clave (ingresos, ventas, ticket promedio).
/// </summary>
public record GetPeriodComparisonQuery(
    DashboardPeriod Period = DashboardPeriod.ThisMonth
) : IRequest<OperationResult<PeriodComparisonDTO>>;
