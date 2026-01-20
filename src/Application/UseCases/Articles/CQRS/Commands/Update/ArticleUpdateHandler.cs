using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Domain.Entities.Articles;

namespace Application.UseCases.Articles.CQRS.Commands.Update;

public sealed class ArticleUpdateHandler
    : IRequestHandler<ArticleUpdateCommand, OperationResult<VoidResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ArticleRules _articleRules;

    public ArticleUpdateHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _articleRules = new ArticleRules(unitOfWork);
    }

    public async Task<OperationResult<VoidResult>> Handle(
        ArticleUpdateCommand request,
        CancellationToken cancellationToken)
    {
        var article = await _articleRules.GetByIdAsync(request.Id, cancellationToken);

        if (article is null)
        {
            return Result.Error(
                ErrorResult.NotFound,
                detail: ArticleMessages.NotFound.WithId(request.Id.ToString()));
        }

        request.Adapt(article);

        var validationResult = await _articleRules.EnsureUniquenessAsync(
            article,
            isUpdate: true,
            cancellationToken);

        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        _unitOfWork.Repository<Article>().Update(article);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
