using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Products.DTOs;

namespace Application.UseCases.Products.CQRS.Queries.Search;

public sealed record ProductSearchQuery(string Term) : IRequest<OperationResult<IEnumerable<ProductDTO>>>;
