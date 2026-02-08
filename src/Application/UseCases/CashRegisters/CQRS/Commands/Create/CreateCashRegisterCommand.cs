using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.CashRegisters.DTOs;

namespace Application.UseCases.CashRegisters.CQRS.Commands.Create;

/// <summary>
/// Command to create a cash register closing.
/// </summary>
public sealed record CreateCashRegisterCommand(
    Guid UserId,
    DateTime OpeningDate,
    DateTime ClosingDate,
    decimal InitialCash,
    decimal FinalCash,
    string? Notes
) : IRequest<OperationResult<CashRegisterReportDTO>>;
