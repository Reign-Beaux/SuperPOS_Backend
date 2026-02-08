namespace Application.UseCases.CashRegisters.DTOs;

public record CashRegisterDTO(
    Guid Id,
    Guid UserId,
    string UserName,
    DateTime OpeningDate,
    DateTime ClosingDate,
    decimal InitialCash,
    decimal FinalCash,
    decimal TotalSales,
    int TotalTransactions,
    int TotalItemsSold,
    decimal Difference,
    string? Notes,
    DateTime CreatedAt
);
