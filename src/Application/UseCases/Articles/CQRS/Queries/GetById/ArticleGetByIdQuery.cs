using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Articles.DTOs;

namespace Application.UseCases.Articles.CQRS.Queries.GetById;

public record ArticleGetByIdQuery(Guid Id) : IRequest<OperationResult<ArticleDTO>>;
