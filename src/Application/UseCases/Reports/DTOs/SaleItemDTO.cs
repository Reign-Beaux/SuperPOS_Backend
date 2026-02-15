namespace Application.UseCases.Reports.DTOs;

/// <summary>
/// Detalle de una venta individual para exportación en Excel.
/// </summary>
/// <param name="SaleId">ID de la venta.</param>
/// <param name="Date">Fecha y hora de la venta.</param>
/// <param name="CustomerName">Nombre del cliente.</param>
/// <param name="UserName">Nombre del usuario (vendedor).</param>
/// <param name="ProductsCount">Cantidad de productos diferentes.</param>
/// <param name="TotalItems">Cantidad total de artículos vendidos.</param>
/// <param name="Subtotal">Subtotal sin impuestos.</param>
/// <param name="Tax">Impuestos (IVA 16%).</param>
/// <param name="Total">Total de la venta.</param>
public record SaleItemDTO(
    Guid SaleId,
    DateTime Date,
    string CustomerName,
    string UserName,
    int ProductsCount,
    int TotalItems,
    decimal Subtotal,
    decimal Tax,
    decimal Total
);
