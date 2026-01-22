using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Products.DTOs;

namespace Application.UseCases.Products.CQRS.Queries.GetById;

public record ProductGetByIdQuery(Guid Id) : IRequest<OperationResult<ProductDTO>>;
