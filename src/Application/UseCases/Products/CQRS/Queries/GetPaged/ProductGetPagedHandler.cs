using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Products.DTOs;
using Domain.Entities.Products;
using Domain.Repositories;
using Domain.Specifications;
using Domain.Specifications.Products;

namespace Application.UseCases.Products.CQRS.Queries.GetPaged;

/// <summary>
/// Handler that demonstrates complete Specification pattern usage for pagination.
/// Shows how to get both paginated results AND total count efficiently.
/// </summary>
public class ProductGetPagedHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<ProductGetPagedQuery, OperationResult<PagedProductsDTO>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<OperationResult<PagedProductsDTO>> Handle(
        ProductGetPagedQuery request,
        CancellationToken cancellationToken)
    {
        // Create specification based on search criteria
        BaseSpecification<Product> specification;

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            // Search by name with pagination
            specification = new ProductsByNameSpecification(
                request.SearchTerm,
                request.PageIndex,
                request.PageSize);
        }
        else
        {
            // Get all products with pagination
            specification = new AllProductsSpecification(
                request.PageIndex,
                request.PageSize);
        }

        // Get paginated results using specification
        var products = await _unitOfWork.Products.ListAsync(specification, cancellationToken);

        // Get total count using the SAME specification (without paging)
        // CountAsync only applies the filter criteria, not paging/ordering
        var totalCount = await _unitOfWork.Products.CountAsync(specification, cancellationToken);

        // Map to DTOs
        var productDtos = _mapper.Map<List<ProductDTO>>(products);

        // Calculate total pages
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        // Return paginated result with metadata
        var pagedResult = new PagedProductsDTO(
            Items: productDtos,
            TotalCount: totalCount,
            PageIndex: request.PageIndex,
            PageSize: request.PageSize,
            TotalPages: totalPages
        );

        return Result.Success(pagedResult);
    }
}
