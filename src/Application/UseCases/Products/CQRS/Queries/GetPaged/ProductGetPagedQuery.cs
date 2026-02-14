using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Products.DTOs;

namespace Application.UseCases.Products.CQRS.Queries.GetPaged;

/// <summary>
/// Query to get paginated products with total count.
/// Demonstrates Specification pattern usage for pagination.
/// </summary>
public record ProductGetPagedQuery(
    int PageIndex = 1,
    int PageSize = 10,
    string? SearchTerm = null
) : IRequest<OperationResult<PagedProductsDTO>>;
