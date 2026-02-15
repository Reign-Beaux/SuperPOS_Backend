using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Dashboard.DTOs;

namespace Application.UseCases.Dashboard.CQRS.Queries.GetDailySummary;

/// <summary>
/// Query para obtener el resumen de ventas de un período específico.
/// Soporta períodos predefinidos (Hoy, EstaSemana, EsteMes) o rangos personalizados.
/// </summary>
public record GetDailySummaryQuery(
    DashboardPeriod Period = DashboardPeriod.Today,
    DateTime? CustomStartDate = null,
    DateTime? CustomEndDate = null
) : IRequest<OperationResult<DailySummaryDTO>>;
