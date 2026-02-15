using Domain.Entities.Sales;
using Domain.Specifications;

namespace Domain.Specifications.Sales;

/// <summary>
/// Especificación para ventas no canceladas con carga opcional de detalles.
/// Útil para obtener todas las ventas válidas sin filtro de fechas.
/// </summary>
public class NonCancelledSalesSpecification : BaseSpecification<Sale>
{
    /// <summary>
    /// Crea una especificación para ventas no canceladas.
    /// </summary>
    /// <param name="includeDetails">True para cargar Customer, SaleDetails y Products</param>
    public NonCancelledSalesSpecification(bool includeDetails = false)
        : base(s => !s.IsCancelled)
    {
        if (includeDetails)
        {
            AddInclude(s => s.Customer);
            AddInclude(s => s.SaleDetails);
            AddInclude("SaleDetails.Product");
            SetSplitQuery(true); // Múltiples colecciones
        }

        AddOrderByDescending(s => s.TotalAmount);
        SetTracking(false);
    }
}
