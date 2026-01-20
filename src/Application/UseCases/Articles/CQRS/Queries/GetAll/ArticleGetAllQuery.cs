using Application.DesignPatterns.OperationResults;
using Application.UseCases.Articles.DTOs;
using Application.DesignPatterns.Mediators.Interfaces;

namespace Application.UseCases.Articles.CQRS.Queries.GetAll;

public sealed record ArticleGetAllQuery : IRequest<OperationResult<IEnumerable<ArticleDTO>>>;
