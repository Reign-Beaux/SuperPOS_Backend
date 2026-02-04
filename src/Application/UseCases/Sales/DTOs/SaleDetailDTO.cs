namespace Application.UseCases.Sales.DTOs;

public record SaleDetailDTO(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Total
);
