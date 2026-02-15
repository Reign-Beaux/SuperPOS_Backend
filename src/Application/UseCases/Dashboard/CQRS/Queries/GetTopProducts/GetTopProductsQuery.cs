using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Dashboard.DTOs;

namespace Application.UseCases.Dashboard.CQRS.Queries.GetTopProducts;

/// <summary>
/// Query para obtener los productos más vendidos ordenados por cantidad.
/// Permite filtrar por período de tiempo y número de resultados.
/// </summary>
public record GetTopProductsQuery(
    DashboardPeriod Period = DashboardPeriod.Today,
    DateTime? CustomStartDate = null,
    DateTime? CustomEndDate = null,
    int TopCount = 10
) : IRequest<OperationResult<List<TopProductDTO>>>;
