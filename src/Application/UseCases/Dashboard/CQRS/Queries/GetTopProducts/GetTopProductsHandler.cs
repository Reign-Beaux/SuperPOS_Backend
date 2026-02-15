using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Dashboard.DTOs;
using Domain.Entities.Dashboard;

namespace Application.UseCases.Dashboard.CQRS.Queries.GetTopProducts;

public class GetTopProductsHandler
    : IRequestHandler<GetTopProductsQuery, OperationResult<List<TopProductDTO>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTopProductsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<List<TopProductDTO>>> Handle(
        GetTopProductsQuery request,
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

        // 3. Obtener los productos más vendidos desde el repositorio
        var topProducts = await _unitOfWork.Sales.GetTopProductsAsync(
            startDate,
            endDate,
            request.TopCount,
            cancellationToken);

        // 4. Mapear a DTOs
        var dtos = topProducts.Select(p => new TopProductDTO(
            ProductId: p.ProductId,
            ProductName: p.ProductName,
            QuantitySold: p.Quantity,
            TotalRevenue: p.Revenue,
            TransactionCount: 0  // Nota: requeriría una consulta adicional si se necesita
        )).ToList();

        return Result.Success(dtos);
    }
}
