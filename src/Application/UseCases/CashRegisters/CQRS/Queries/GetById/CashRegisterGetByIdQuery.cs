using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.CashRegisters.DTOs;

namespace Application.UseCases.CashRegisters.CQRS.Queries.GetById;

public sealed record CashRegisterGetByIdQuery(Guid Id)
    : IRequest<OperationResult<CashRegisterDTO>>;
