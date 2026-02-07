namespace Application.UseCases.Inventories.DTOs;

public record InventoryDTO(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string? ProductDescription,
    string? Barcode,
    int Stock,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
