namespace Application.UseCases.Inventories.DTOs;

public record InventoryDTO(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
