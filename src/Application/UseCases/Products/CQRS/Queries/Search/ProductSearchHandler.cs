using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Domain.Repositories;
using Application.UseCases.Products.DTOs;
using Domain.Specifications.Products;

namespace Application.UseCases.Products.CQRS.Queries.Search;

public sealed class ProductSearchHandler : IRequestHandler<ProductSearchQuery, OperationResult<IEnumerable<ProductDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public ProductSearchHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<IEnumerable<ProductDTO>>> Handle(
        ProductSearchQuery request,
        CancellationToken cancellationToken)
    {
        // Use Specification pattern to search products by name with filtering and ordering
        var specification = new ProductsByNameSpecification(request.Term);
        var products = await _unitOfWork.Products.ListAsync(specification, cancellationToken);

        // Note: Original SearchAsync searched by name OR barcode and limited to 20 results.
        // This specification only searches by name. For barcode search, use repository method
        // or create a more comprehensive ProductSearchSpecification.
        var productsDto = _mapper.Map<IEnumerable<ProductDTO>>(products);

        return Result.Success(productsDto);
    }
}
