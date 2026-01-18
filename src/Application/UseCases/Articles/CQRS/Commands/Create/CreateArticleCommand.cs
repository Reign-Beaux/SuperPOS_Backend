using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Articles.DTOs;

namespace Application.UseCases.Articles.CQRS.Commands.Create;

public record CreateArticleCommand(
    string Name,
    string? Description,
    string? Barcode
) : IRequest<OperationResult<ArticleDTO>>;
