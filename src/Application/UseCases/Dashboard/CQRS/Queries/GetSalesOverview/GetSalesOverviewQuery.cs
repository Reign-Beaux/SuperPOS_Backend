using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Dashboard.DTOs;

namespace Application.UseCases.Dashboard.CQRS.Queries.GetSalesOverview;

/// <summary>
/// Query para obtener una vista general completa del dashboard.
/// Combina múltiples analíticas en una sola respuesta ejecutando consultas en paralelo.
/// Incluye: resúmenes diarios/semanales/mensuales, top productos, top clientes y tendencias horarias.
/// </summary>
public record GetSalesOverviewQuery() : IRequest<OperationResult<SalesOverviewDTO>>;
