using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Domain.Repositories;
using Application.UseCases.Products.DTOs;
using Domain.Entities.Products;
using Domain.Specifications.Products;

namespace Application.UseCases.Products.CQRS.Queries.GetAll;

public sealed class ProductGetAllHandler : IRequestHandler<ProductGetAllQuery, OperationResult<IEnumerable<ProductDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public ProductGetAllHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<IEnumerable<ProductDTO>>> Handle(
        ProductGetAllQuery request,
        CancellationToken cancellationToken)
    {
        // Use Specification pattern to get all products with ordering
        var specification = new AllProductsSpecification();
        var products = await _unitOfWork.Products.ListAsync(specification, cancellationToken);

        var productsDto = _mapper.Map<IEnumerable<ProductDTO>>(products);

        return Result.Success(productsDto);
    }
}
