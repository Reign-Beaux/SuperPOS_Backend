using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence;
using Application.Interfaces.Persistence.UnitOfWorks;
using Application.UseCases.Articles.DTOs;
using Domain.Entities.Articles;

namespace Application.UseCases.Articles.CQRS.Queries.GetAll;

public sealed class ArticleGetAllHandler : IRequestHandler<ArticleGetAllQuery, OperationResult<IEnumerable<ArticleDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Article> _articleRepository;

    public ArticleGetAllHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _articleRepository = unitOfWork.Repository<Article>();
    }

    public async Task<OperationResult<IEnumerable<ArticleDTO>>> Handle(
        ArticleGetAllQuery request,
        CancellationToken cancellationToken)
    {
        var articles = await _articleRepository.GetAllAsync(cancellationToken);

        var articlesDto = _mapper.Map<IEnumerable<ArticleDTO>>(articles);

        return Result.Success(articlesDto);
    }
}
