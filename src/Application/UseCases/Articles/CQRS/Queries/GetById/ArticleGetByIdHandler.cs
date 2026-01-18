using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence;
using Application.Interfaces.Persistence.UnitOfWorks;
using Application.UseCases.Articles.DTOs;
using Domain.Entities.Articles;
using MapsterMapper;

namespace Application.UseCases.Articles.CQRS.Queries.GetById;

public sealed class ArticleGetByIdHandler : IRequestHandler<ArticleGetByIdQuery, OperationResult<ArticleDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Article> _articleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ArticleGetByIdHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _articleRepository = _unitOfWork.Repository<Article>();
    }

    public async Task<OperationResult<ArticleDTO>> Handle(
        ArticleGetByIdQuery request,
        CancellationToken cancellationToken)
    {
        var article = await _articleRepository.GetByIdAsync(request.Id, cancellationToken);

        if (article is null)
            return Result.Error(
                ErrorResult.NotFound,
                detail: ArticleMessages.NotFound.WithId(request.Id.ToString()));

        var articleDto = _mapper.Map<ArticleDTO>(article);

        return Result.Success(articleDto);
    }
}
