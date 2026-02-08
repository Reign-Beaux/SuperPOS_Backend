using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.CashRegisters.DTOs;

namespace Application.UseCases.CashRegisters.CQRS.Queries.GetAll;

public sealed record CashRegisterGetAllQuery()
    : IRequest<OperationResult<IEnumerable<CashRegisterDTO>>>;
