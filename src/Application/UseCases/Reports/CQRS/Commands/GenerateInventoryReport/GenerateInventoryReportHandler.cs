using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Services;
using Application.UseCases.Reports.DTOs;
using Domain.Entities.Products;

namespace Application.UseCases.Reports.CQRS.Commands.GenerateInventoryReport;

public class GenerateInventoryReportHandler(
    IUnitOfWork unitOfWork,
    IReportService reportService)
    : IRequestHandler<GenerateInventoryReportCommand, OperationResult<FileResultDTO>>
{
    // Umbral de stock bajo (sincronizado con StockSettings:LowStockThreshold en configuración)
    private const int LowStockThreshold = 10;

    public async Task<OperationResult<FileResultDTO>> Handle(
        GenerateInventoryReportCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Usar el umbral de stock bajo
        var lowStockThreshold = LowStockThreshold;

        // 2. Obtener todos los productos con inventario
        var products = await unitOfWork.Repository<Product>().GetAllAsync(cancellationToken);

        // 3. Obtener inventarios de todos los productos
        var inventories = await unitOfWork.Inventories.GetAllAsync(cancellationToken);
        var inventoryDict = inventories.ToDictionary(i => i.ProductId, i => i);

        // 4. Construir lista de items con información completa
        var allItems = products.Select(p =>
        {
            var inventory = inventoryDict.GetValueOrDefault(p.Id);
            var stock = inventory?.Stock ?? 0;
            var isLowStock = stock > 0 && stock <= lowStockThreshold;
            var isOutOfStock = stock == 0;
            var totalValue = stock * p.UnitPrice;

            return new InventoryItemDTO(
                ProductId: p.Id,
                ProductName: p.Name,
                Barcode: p.Barcode ?? "N/A",
                Stock: stock,
                Price: p.UnitPrice,
                TotalValue: totalValue,
                IsLowStock: isLowStock,
                IsOutOfStock: isOutOfStock
            );
        }).ToList();

        // 5. Filtrar items por categorías
        var outOfStockItems = allItems.Where(i => i.IsOutOfStock).ToList();
        var lowStockItems = allItems.Where(i => i.IsLowStock).ToList();

        // 6. Calcular métricas resumen
        var totalProducts = allItems.Count;
        var totalStock = allItems.Sum(i => i.Stock);
        var totalValue = allItems.Sum(i => i.TotalValue);
        var lowStockCount = lowStockItems.Count;
        var outOfStockCount = outOfStockItems.Count;

        // 7. Construir el DTO del reporte
        var reportTitle = "Reporte de Inventario";
        var reportData = new InventoryReportDTO(
            ReportTitle: reportTitle,
            GeneratedAt: DateTime.UtcNow,
            TotalProducts: totalProducts,
            TotalStock: totalStock,
            TotalValue: totalValue,
            LowStockCount: lowStockCount,
            OutOfStockCount: outOfStockCount,
            LowStockItems: lowStockItems,
            OutOfStockItems: outOfStockItems,
            AllItems: request.IncludeAllItems ? allItems : null
        );

        // 8. Generar el archivo (PDF o Excel)
        byte[] fileBytes;
        string fileName;
        string contentType;

        if (request.Format == ExportFormat.Pdf)
        {
            fileBytes = await reportService.GenerateInventoryReportPdfAsync(reportData, cancellationToken);
            fileName = $"Reporte_Inventario_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
            contentType = "application/pdf";
        }
        else
        {
            fileBytes = await reportService.GenerateInventoryReportExcelAsync(reportData, cancellationToken);
            fileName = $"Reporte_Inventario_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        }

        return Result.Success(new FileResultDTO(fileBytes, fileName, contentType));
    }
}
