using Domain.Entities.Sales;
using Domain.Specifications;

namespace Domain.Specifications.Sales;

/// <summary>
/// Especificación reutilizable para filtrar ventas por rango de fechas.
/// Filtra ventas no canceladas dentro del período especificado.
/// </summary>
public class SalesByDateRangeSpecification : BaseSpecification<Sale>
{
    /// <summary>
    /// Crea una especificación para ventas en un rango de fechas (sin detalles cargados).
    /// </summary>
    /// <param name="startDate">Fecha de inicio (inclusiva)</param>
    /// <param name="endDate">Fecha de fin (exclusiva)</param>
    public SalesByDateRangeSpecification(DateTime startDate, DateTime endDate)
        : base(s => !s.IsCancelled && s.CreatedAt >= startDate && s.CreatedAt < endDate)
    {
        AddOrderByDescending(s => s.CreatedAt);
        SetTracking(false);  // Read-only para analytics
        SetSplitQuery(false); // Sin navegación, no necesita split query
    }

    /// <summary>
    /// Crea una especificación para ventas con opción de cargar detalles relacionados.
    /// </summary>
    /// <param name="startDate">Fecha de inicio (inclusiva)</param>
    /// <param name="endDate">Fecha de fin (exclusiva)</param>
    /// <param name="includeDetails">True para cargar SaleDetails y Products</param>
    public SalesByDateRangeSpecification(DateTime startDate, DateTime endDate, bool includeDetails)
        : base(s => !s.IsCancelled && s.CreatedAt >= startDate && s.CreatedAt < endDate)
    {
        if (includeDetails)
        {
            AddInclude(s => s.SaleDetails);
            AddInclude("SaleDetails.Product");
            SetSplitQuery(true); // Múltiples colecciones - prevenir explosión cartesiana
        }

        AddOrderByDescending(s => s.CreatedAt);
        SetTracking(false);
    }
}
