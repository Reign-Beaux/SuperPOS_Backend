using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence;
using Application.Interfaces.Persistence.UnitOfWorks;
using Application.UseCases.Products.DTOs;
using Domain.Entities.Products;

namespace Application.UseCases.Products.CQRS.Queries.GetAll;

public sealed class ProductGetAllHandler : IRequestHandler<ProductGetAllQuery, OperationResult<IEnumerable<ProductDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Product> _productRepository;

    public ProductGetAllHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _productRepository = unitOfWork.Repository<Product>();
    }

    public async Task<OperationResult<IEnumerable<ProductDTO>>> Handle(
        ProductGetAllQuery request,
        CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetAllAsync(cancellationToken);

        var productsDto = _mapper.Map<IEnumerable<ProductDTO>>(products);

        return Result.Success(productsDto);
    }
}
