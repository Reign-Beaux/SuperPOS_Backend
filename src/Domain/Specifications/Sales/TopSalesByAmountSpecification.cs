using Domain.Entities.Sales;
using Domain.Specifications;

namespace Domain.Specifications.Sales;

/// <summary>
/// Especificación para obtener las N ventas más altas por monto dentro de un rango de fechas.
/// Útil para análisis de ventas top o reportes de mejores transacciones.
/// </summary>
public class TopSalesByAmountSpecification : BaseSpecification<Sale>
{
    /// <summary>
    /// Crea una especificación para las N ventas más altas.
    /// </summary>
    /// <param name="startDate">Fecha de inicio (inclusiva)</param>
    /// <param name="endDate">Fecha de fin (exclusiva)</param>
    /// <param name="topCount">Número de ventas a retornar (default: 10)</param>
    public TopSalesByAmountSpecification(DateTime startDate, DateTime endDate, int topCount = 10)
        : base(s => !s.IsCancelled && s.CreatedAt >= startDate && s.CreatedAt < endDate)
    {
        AddInclude(s => s.Customer);
        AddOrderByDescending(s => s.TotalAmount);
        ApplyPaging(0, topCount);

        SetTracking(false);
        SetSplitQuery(false); // Solo Customer, no múltiples colecciones
    }
}
