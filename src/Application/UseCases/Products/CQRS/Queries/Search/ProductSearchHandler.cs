using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Domain.Repositories;
using Application.UseCases.Products.DTOs;

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
        var products = await _unitOfWork.Products.SearchAsync(
            request.Term,
            maxResults: 20,
            cancellationToken);

        var productsDto = _mapper.Map<IEnumerable<ProductDTO>>(products);

        return Result.Success(productsDto);
    }
}
