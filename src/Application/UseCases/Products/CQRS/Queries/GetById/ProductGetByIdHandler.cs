using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence;
using Application.Interfaces.Persistence.UnitOfWorks;
using Application.UseCases.Products.DTOs;
using Domain.Entities.Products;
using MapsterMapper;

namespace Application.UseCases.Products.CQRS.Queries.GetById;

public sealed class ProductGetByIdHandler : IRequestHandler<ProductGetByIdQuery, OperationResult<ProductDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductGetByIdHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _productRepository = _unitOfWork.Repository<Product>();
    }

    public async Task<OperationResult<ProductDTO>> Handle(
        ProductGetByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
            return Result.Error(
                ErrorResult.NotFound,
                detail: ProductMessages.NotFound.WithId(request.Id.ToString()));

        var productDto = _mapper.Map<ProductDTO>(product);

        return Result.Success(productDto);
    }
}
