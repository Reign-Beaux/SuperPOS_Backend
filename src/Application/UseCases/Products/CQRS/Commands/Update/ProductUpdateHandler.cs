using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Domain.Entities.Products;

namespace Application.UseCases.Products.CQRS.Commands.Update;

public sealed class ProductUpdateHandler
    : IRequestHandler<ProductUpdateCommand, OperationResult<VoidResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ProductRules _productRules;

    public ProductUpdateHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _productRules = new ProductRules(unitOfWork);
    }

    public async Task<OperationResult<VoidResult>> Handle(
        ProductUpdateCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRules.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
        {
            return Result.Error(
                ErrorResult.NotFound,
                detail: ProductMessages.NotFound.WithId(request.Id.ToString()));
        }

        request.Adapt(product);

        var validationResult = await _productRules.EnsureUniquenessAsync(
            product,
            isUpdate: true,
            cancellationToken);

        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        _unitOfWork.Repository<Product>().Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
