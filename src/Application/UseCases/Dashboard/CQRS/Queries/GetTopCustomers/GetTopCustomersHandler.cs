using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Dashboard.DTOs;
using Domain.Entities.Dashboard;
using Domain.Specifications.Sales;

namespace Application.UseCases.Dashboard.CQRS.Queries.GetTopCustomers;

public class GetTopCustomersHandler
    : IRequestHandler<GetTopCustomersQuery, OperationResult<List<TopCustomerDTO>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTopCustomersHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<List<TopCustomerDTO>>> Handle(
        GetTopCustomersQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Validar el número de resultados
        if (request.TopCount < 1 || request.TopCount > 50)
        {
            return Result.Error(
                ErrorResult.BadRequest,
                detail: DashboardMessages.InvalidTopCount);
        }

        // 2. Calcular el rango de fechas
        var (startDate, endDate) = DashboardPeriodHelper.GetDateRange(
            request.Period,
            request.CustomStartDate,
            request.CustomEndDate);

        // 3. Obtener ventas con información de clientes usando especificación
        var specification = new SalesByDateRangeSpecification(startDate, endDate);
        var sales = await _unitOfWork.Sales.ListAsync(specification, cancellationToken);

        // 4. Cargar información de clientes
        var customerIds = sales.Select(s => s.CustomerId).Distinct();
        var customers = await Task.WhenAll(
            customerIds.Select(id => _unitOfWork.Customers.GetByIdAsync(id, cancellationToken)));

        // 5. Agrupar por cliente y calcular métricas
        var topCustomers = sales
            .GroupBy(s => s.CustomerId)
            .Select(g => new
            {
                CustomerId = g.Key,
                PurchaseCount = g.Count(),
                TotalSpent = g.Sum(s => s.TotalAmount),
                AvgTicketSize = g.Average(s => s.TotalAmount),
                LastPurchaseDate = g.Max(s => s.CreatedAt)
            })
            .OrderByDescending(x => x.TotalSpent)
            .Take(request.TopCount)
            .ToList();

        // 6. Mapear a DTOs
        var dtos = topCustomers.Select(tc =>
        {
            var customer = customers.FirstOrDefault(c => c?.Id == tc.CustomerId);
            return new TopCustomerDTO(
                CustomerId: tc.CustomerId,
                CustomerName: customer?.Name ?? "Desconocido",
                PurchaseCount: tc.PurchaseCount,
                TotalSpent: tc.TotalSpent,
                AverageTicketSize: tc.AvgTicketSize,
                LastPurchaseDate: tc.LastPurchaseDate
            );
        }).ToList();

        return Result.Success(dtos);
    }
}
