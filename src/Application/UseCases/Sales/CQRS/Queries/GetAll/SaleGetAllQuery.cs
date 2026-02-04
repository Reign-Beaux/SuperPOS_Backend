using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Sales.DTOs;

namespace Application.UseCases.Sales.CQRS.Queries.GetAll;

public record SaleGetAllQuery : IRequest<OperationResult<List<SaleDTO>>>;
