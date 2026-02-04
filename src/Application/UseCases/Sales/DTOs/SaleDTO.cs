namespace Application.UseCases.Sales.DTOs;

public record SaleDTO(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    Guid UserId,
    string UserName,
    decimal TotalAmount,
    DateTime CreatedAt,
    List<SaleDetailDTO> Details
);
