using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Sales.DTOs;

namespace Application.UseCases.Sales.CQRS.Queries.GetById;

public record SaleGetByIdQuery(Guid Id) : IRequest<OperationResult<SaleDTO>>;
