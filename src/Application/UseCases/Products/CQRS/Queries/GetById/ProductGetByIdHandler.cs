using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Products.DTOs;
using Domain.Entities.Products;
using Domain.Repositories;

namespace Application.UseCases.Products.CQRS.Queries.GetById;

public sealed class ProductGetByIdHandler : IRequestHandler<ProductGetByIdQuery, OperationResult<ProductDTO>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public ProductGetByIdHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<ProductDTO>> Handle(
        ProductGetByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Use specific repository through UnitOfWork property
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
            return Result.Error(
                ErrorResult.NotFound,
                detail: ProductMessages.NotFound.WithId(request.Id.ToString()));

        var productDto = _mapper.Map<ProductDTO>(product);

        return Result.Success(productDto);
    }
}
