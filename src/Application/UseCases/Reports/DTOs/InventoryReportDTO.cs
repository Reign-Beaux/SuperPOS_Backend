namespace Application.UseCases.Reports.DTOs;

/// <summary>
/// Datos completos del reporte de inventario.
/// </summary>
/// <param name="ReportTitle">Título del reporte.</param>
/// <param name="GeneratedAt">Fecha y hora de generación.</param>
/// <param name="TotalProducts">Total de productos registrados.</param>
/// <param name="TotalStock">Suma total de unidades en stock.</param>
/// <param name="TotalValue">Valor total del inventario.</param>
/// <param name="LowStockCount">Cantidad de productos con stock bajo.</param>
/// <param name="OutOfStockCount">Cantidad de productos agotados.</param>
/// <param name="LowStockItems">Productos con stock bajo.</param>
/// <param name="OutOfStockItems">Productos agotados.</param>
/// <param name="AllItems">Todos los productos (opcional, solo para Excel).</param>
public record InventoryReportDTO(
    string ReportTitle,
    DateTime GeneratedAt,
    int TotalProducts,
    int TotalStock,
    decimal TotalValue,
    int LowStockCount,
    int OutOfStockCount,
    List<InventoryItemDTO> LowStockItems,
    List<InventoryItemDTO> OutOfStockItems,
    List<InventoryItemDTO>? AllItems
);
