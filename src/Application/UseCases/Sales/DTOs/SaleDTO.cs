namespace Application.UseCases.Sales.DTOs;

public record SaleDTO(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    Guid UserId,
    string UserName,
    decimal TotalAmount,
    DateTime CreatedAt,
    bool IsCancelled,
    DateTime? CancelledAt,
    Guid? CancelledByUserId,
    string? CancellationReason,
    List<SaleDetailDTO> Details
);
