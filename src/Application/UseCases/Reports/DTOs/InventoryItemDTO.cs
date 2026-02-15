namespace Application.UseCases.Reports.DTOs;

/// <summary>
/// Detalle de un producto del inventario para exportación.
/// </summary>
/// <param name="ProductId">ID del producto.</param>
/// <param name="ProductName">Nombre del producto.</param>
/// <param name="Barcode">Código de barras.</param>
/// <param name="Stock">Cantidad en stock.</param>
/// <param name="Price">Precio unitario.</param>
/// <param name="TotalValue">Valor total (Stock × Precio).</param>
/// <param name="IsLowStock">Indica si el stock está bajo.</param>
/// <param name="IsOutOfStock">Indica si está agotado.</param>
public record InventoryItemDTO(
    Guid ProductId,
    string ProductName,
    string Barcode,
    int Stock,
    decimal Price,
    decimal TotalValue,
    bool IsLowStock,
    bool IsOutOfStock
);
