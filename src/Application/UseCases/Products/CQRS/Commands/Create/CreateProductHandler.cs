using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Application.UseCases.Products.DTOs;
using Domain.Entities.Products;
using MapsterMapper;

namespace Application.UseCases.Products.CQRS.Commands.Create;

public sealed class CreateProductHandler
    : IRequestHandler<CreateProductCommand, OperationResult<ProductDTO>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ProductRules _productRules;

    public CreateProductHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _productRules = new ProductRules(unitOfWork);
    }

    public async Task<OperationResult<ProductDTO>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = _mapper.Map<Product>(request);

        var validationResult = await _productRules.EnsureUniquenessAsync(
            product,
            isUpdate: false,
            cancellationToken);

        if (!validationResult.IsSuccess)
            return validationResult;

        _unitOfWork.Repository<Product>().Add(product);

        var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (affectedRows == 0)
            return Result.Error(
                ErrorResult.BadRequest,
                detail: ProductMessages.Create.Failed);

        var productDto = _mapper.Map<ProductDTO>(product);

        return new OperationResult<ProductDTO>(StatusResult.Created, productDto);
    }
}
