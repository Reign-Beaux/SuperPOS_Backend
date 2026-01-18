using Application.UseCases.Articles.DTOs;
using Domain.Entities.Articles;
using Mapster;

namespace Application.UseCases.Articles.Mappings;

public class ArticleMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Article, ArticleDTO>();
    }
}
