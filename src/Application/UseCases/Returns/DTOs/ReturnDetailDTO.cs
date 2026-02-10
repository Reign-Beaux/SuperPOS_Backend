namespace Application.UseCases.Returns.DTOs;

public record ReturnDetailDTO(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Total,
    string? Condition
);
