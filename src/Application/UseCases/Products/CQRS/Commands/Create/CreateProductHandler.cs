using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Domain.Entities.Products;
using Domain.Services;
using Domain.ValueObjects;

namespace Application.UseCases.Products.CQRS.Commands.Create;

public sealed class CreateProductHandler
    : IRequestHandler<CreateProductCommand, OperationResult<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductUniquenessChecker _uniquenessChecker;

    public CreateProductHandler(
        IUnitOfWork unitOfWork,
        IProductUniquenessChecker uniquenessChecker)
    {
        _unitOfWork = unitOfWork;
        _uniquenessChecker = uniquenessChecker;
    }

    public async Task<OperationResult<Guid>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        // Validate uniqueness using domain service
        var isNameUnique = await _uniquenessChecker.IsNameUniqueAsync(
            request.Name,
            cancellationToken: cancellationToken);

        if (!isNameUnique)
            return Result.Error(
                ErrorResult.Exists,
                detail: ProductMessages.AlreadyExists.WithName(request.Name));

        // Validate barcode uniqueness if provided
        if (!string.IsNullOrWhiteSpace(request.Barcode))
        {
            var isBarcodeUnique = await _uniquenessChecker.IsBarcodeUniqueAsync(
                request.Barcode,
                cancellationToken: cancellationToken);

            if (!isBarcodeUnique)
                return Result.Error(
                    ErrorResult.Exists,
                    detail: ProductMessages.AlreadyExists.WithBarcode(request.Barcode));
        }

        // Create value objects
        var barcode = string.IsNullOrWhiteSpace(request.Barcode)
            ? null
            : Barcode.Create(request.Barcode);

        // Use domain factory method
        var product = Product.Create(
            request.Name,
            request.Description,
            barcode,
            request.UnitPrice);

        // Use specific repository
        _unitOfWork.Products.Add(product);

        var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (affectedRows == 0)
            return Result.Error(
                ErrorResult.BadRequest,
                detail: ProductMessages.Create.Failed);

        return Result.Created(product.Id);
    }
}
