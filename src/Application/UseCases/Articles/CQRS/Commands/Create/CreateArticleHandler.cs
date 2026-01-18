using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Application.UseCases.Articles.DTOs;
using Domain.Entities.Articles;
using MapsterMapper;

namespace Application.UseCases.Articles.CQRS.Commands.Create;

public sealed class CreateArticleHandler
    : IRequestHandler<CreateArticleCommand, OperationResult<ArticleDTO>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ArticleRules _articleRules;

    public CreateArticleHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _articleRules = new ArticleRules(unitOfWork);
    }

    public async Task<OperationResult<ArticleDTO>> Handle(
        CreateArticleCommand request,
        CancellationToken cancellationToken)
    {
        // Create Article entity from command
        var article = new Article
        {
            Name = request.Name,
            Description = request.Description ?? string.Empty,
            Barcode = request.Barcode
        };

        // Validate uniqueness
        var validationResult = await _articleRules.EnsureUniquenessAsync(
            article,
            isUpdate: false,
            cancellationToken);

        if (!validationResult.IsSuccess)
            return validationResult;

        // Add to repository
        _unitOfWork.Repository<Article>().Add(article);

        // Save changes
        var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (affectedRows == 0)
            return Result.Error(
                ErrorResult.BadRequest,
                detail: ArticleMessages.Create.Failed);

        // Map to DTO and return
        var articleDto = _mapper.Map<ArticleDTO>(article);

        return new OperationResult<ArticleDTO>(StatusResult.Created, articleDto);
    }
}
