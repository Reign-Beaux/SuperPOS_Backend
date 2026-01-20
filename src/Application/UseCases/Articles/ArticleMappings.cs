using Application.UseCases.Articles.CQRS.Commands.Update;
using Application.UseCases.Articles.DTOs;
using Domain.Entities.Articles;

namespace Application.UseCases.Articles;

public class ArticleMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Article, ArticleDTO>();
        config.NewConfig<ArticleUpdateCommand, Article>();
    }
}
