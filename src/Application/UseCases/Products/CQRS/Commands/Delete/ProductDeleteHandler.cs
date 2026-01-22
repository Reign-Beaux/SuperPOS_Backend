using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Domain.Entities.Products;

namespace Application.UseCases.Products.CQRS.Commands.Delete;

public sealed class ProductDeleteHandler
    : IRequestHandler<ProductDeleteCommand, OperationResult<VoidResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ProductRules _productRules;

    public ProductDeleteHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _productRules = new ProductRules(unitOfWork);
    }

    public async Task<OperationResult<VoidResult>> Handle(
        ProductDeleteCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRules.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
        {
            return Result.Error(
                ErrorResult.NotFound,
                detail: ProductMessages.NotFound.WithId(request.Id.ToString()));
        }

        _unitOfWork.Repository<Product>().Delete(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
