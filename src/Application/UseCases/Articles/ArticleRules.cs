using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Domain.Entities.Articles;

namespace Application.UseCases.Articles;

internal sealed class ArticleRules(IUnitOfWork unitOfWork)
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<OperationResult<VoidResult>> EnsureUniquenessAsync(
        Article newArticle,
        bool isUpdate,
        CancellationToken cancellationToken)
    {
        var newBarcode = string.IsNullOrWhiteSpace(newArticle.Barcode) ? null : newArticle.Barcode;

        var currentArticle = await _unitOfWork.Repository<Article>().FirstOrDefaultAsync(
            predicate: isUpdate
                ? a =>
                    a.Id != newArticle.Id &&
                    (a.Name == newArticle.Name || (newBarcode != null && a.Barcode == newBarcode))
                : a =>
                    a.Name == newArticle.Name || (newBarcode != null && a.Barcode == newBarcode),
            cancellationToken
        );

        if (currentArticle is null)
            return Result.Success();

        if (currentArticle.Name == newArticle.Name)
            return Result.Error(
                ErrorResult.Exists,
                detail: ArticleMessages.AlreadyExists.WithName(newArticle.Name));

        if (!string.IsNullOrWhiteSpace(newBarcode) && currentArticle.Barcode == newBarcode)
            return Result.Error(
                ErrorResult.Exists,
                detail: ArticleMessages.AlreadyExists.WithBarcode(newBarcode));

        return Result.Success();
    }
}
