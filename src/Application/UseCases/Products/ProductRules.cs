using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Domain.Entities.Products;

namespace Application.UseCases.Products;

internal sealed class ProductRules(IUnitOfWork unitOfWork)
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<OperationResult<VoidResult>> EnsureUniquenessAsync(
        Product newProduct,
        bool isUpdate,
        CancellationToken cancellationToken)
    {
        var newBarcode = string.IsNullOrWhiteSpace(newProduct.Barcode) ? null : newProduct.Barcode;

        var currentProduct = await _unitOfWork.Repository<Product>().FirstOrDefaultAsync(
            predicate: isUpdate
                ? a =>
                    a.Id != newProduct.Id &&
                    (a.Name == newProduct.Name || (newBarcode != null && a.Barcode == newBarcode))
                : a =>
                    a.Name == newProduct.Name || (newBarcode != null && a.Barcode == newBarcode),
            cancellationToken
        );

        if (currentProduct is null)
            return Result.Success();

        if (currentProduct.Name == newProduct.Name)
            return Result.Error(
                ErrorResult.Exists,
                detail: ProductMessages.AlreadyExists.WithName(newProduct.Name));

        if (!string.IsNullOrWhiteSpace(newBarcode) && currentProduct.Barcode == newBarcode)
            return Result.Error(
                ErrorResult.Exists,
                detail: ProductMessages.AlreadyExists.WithBarcode(newBarcode));

        return Result.Success();
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<Product>().GetByIdAsync(id, cancellationToken);
    }
}
