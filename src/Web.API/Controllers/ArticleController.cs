using Application.DesignPatterns.Mediators.Interfaces;
using Application.UseCases.Articles.CQRS.Commands.Create;
using Application.UseCases.Articles.CQRS.Queries.GetById;

namespace Web.API.Controllers;

[Route("api/[controller]")]
public class ArticleController(IMediator mediator) : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateArticleCommand command)
    {
        var result = await mediator.Send(command);
        return HandleResult(result, nameof(GetById));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        ArticleGetByIdQuery request = new(id);
        var result = await mediator.Send(request);
        return HandleResult(result);
    }
}
