using Domain.Entities.Returns;

namespace Application.UseCases.Returns.DTOs;

public record ReturnDTO(
    Guid Id,
    Guid SaleId,
    Guid CustomerId,
    string CustomerName,
    Guid ProcessedByUserId,
    string ProcessedByUserName,
    decimal TotalRefund,
    ReturnType Type,
    string Reason,
    ReturnStatus Status,
    Guid? ApprovedByUserId,
    DateTime? ApprovedAt,
    Guid? RejectedByUserId,
    DateTime? RejectedAt,
    string? RejectionReason,
    DateTime CreatedAt,
    List<ReturnDetailDTO> ReturnDetails
);
