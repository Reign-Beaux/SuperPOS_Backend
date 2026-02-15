using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Dashboard.DTOs;

namespace Application.UseCases.Dashboard.CQRS.Queries.GetTopCustomers;

/// <summary>
/// Query para obtener los clientes más frecuentes ordenados por total gastado.
/// Permite filtrar por período de tiempo y número de resultados.
/// </summary>
public record GetTopCustomersQuery(
    DashboardPeriod Period = DashboardPeriod.ThisMonth,
    DateTime? CustomStartDate = null,
    DateTime? CustomEndDate = null,
    int TopCount = 10
) : IRequest<OperationResult<List<TopCustomerDTO>>>;
