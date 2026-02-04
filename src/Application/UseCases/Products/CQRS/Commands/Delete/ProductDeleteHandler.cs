using Domain.Entities.Products;
using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Domain.Repositories;

namespace Application.UseCases.Products.CQRS.Commands.Delete;

public sealed class ProductDeleteHandler
    : IRequestHandler<ProductDeleteCommand, OperationResult<VoidResult>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductDeleteHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<VoidResult>> Handle(
        ProductDeleteCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
        {
            return Result.Error(
                ErrorResult.NotFound,
                detail: ProductMessages.NotFound.WithId(request.Id.ToString()));
        }

        _unitOfWork.Products.Delete(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
