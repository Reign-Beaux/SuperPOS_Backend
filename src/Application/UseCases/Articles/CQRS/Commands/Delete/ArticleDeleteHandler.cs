using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Domain.Entities.Articles;

namespace Application.UseCases.Articles.CQRS.Commands.Delete;

public sealed class ArticleDeleteHandler
    : IRequestHandler<ArticleDeleteCommand, OperationResult<VoidResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ArticleRules _articleRules;

    public ArticleDeleteHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _articleRules = new ArticleRules(unitOfWork);
    }

    public async Task<OperationResult<VoidResult>> Handle(
        ArticleDeleteCommand request,
        CancellationToken cancellationToken)
    {
        var article = await _articleRules.GetByIdAsync(request.Id, cancellationToken);

        if (article is null)
        {
            return Result.Error(
                ErrorResult.NotFound,
                detail: ArticleMessages.NotFound.WithId(request.Id.ToString()));
        }

        _unitOfWork.Repository<Article>().Delete(article);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
