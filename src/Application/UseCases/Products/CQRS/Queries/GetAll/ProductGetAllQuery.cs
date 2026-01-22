using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Products.DTOs;

namespace Application.UseCases.Products.CQRS.Queries.GetAll;

public sealed record ProductGetAllQuery : IRequest<OperationResult<IEnumerable<ProductDTO>>>;
