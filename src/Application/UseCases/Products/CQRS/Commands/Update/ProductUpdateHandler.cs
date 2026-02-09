using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Domain.Entities.Products;
using Domain.Repositories;
using Domain.Services;
using Domain.ValueObjects;

namespace Application.UseCases.Products.CQRS.Commands.Update;

public sealed class ProductUpdateHandler
    : IRequestHandler<ProductUpdateCommand, OperationResult<VoidResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductUniquenessChecker _uniquenessChecker;

    public ProductUpdateHandler(IUnitOfWork unitOfWork, IProductUniquenessChecker uniquenessChecker)
    {
        _unitOfWork = unitOfWork;
        _uniquenessChecker = uniquenessChecker;
    }

    public async Task<OperationResult<VoidResult>> Handle(
        ProductUpdateCommand request,
        CancellationToken cancellationToken)
    {
        // Get product using specific repository
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
        {
            return Result.Error(
                ErrorResult.NotFound,
                detail: ProductMessages.NotFound.WithId(request.Id.ToString()));
        }

        // Validate name uniqueness if changed
        if (product.Name != request.Name)
        {
            var isNameUnique = await _uniquenessChecker.IsNameUniqueAsync(
                request.Name,
                excludeId: request.Id,
                cancellationToken);

            if (!isNameUnique)
            {
                return Result.Error(
                    ErrorResult.Exists,
                    detail: ProductMessages.AlreadyExists.WithName(request.Name));
            }
        }

        // Validate barcode uniqueness if changed
        if (product.Barcode != request.Barcode && !string.IsNullOrWhiteSpace(request.Barcode))
        {
            var isBarcodeUnique = await _uniquenessChecker.IsBarcodeUniqueAsync(
                request.Barcode,
                excludeId: request.Id,
                cancellationToken);

            if (!isBarcodeUnique)
            {
                return Result.Error(
                    ErrorResult.Exists,
                    detail: ProductMessages.AlreadyExists.WithBarcode(request.Barcode));
            }
        }

        // Use domain methods to update product
        product.UpdateInfo(request.Name, request.Description);

        var barcode = string.IsNullOrWhiteSpace(request.Barcode)
            ? null
            : Barcode.Create(request.Barcode);
        product.UpdateBarcode(barcode);

        product.UpdatePrice(request.UnitPrice);

        // Use specific repository
        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
