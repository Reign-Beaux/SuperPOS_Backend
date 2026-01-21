using Application.DesignPatterns.Mediators.Interfaces;
using Application.UseCases.Articles.CQRS.Commands.Create;
using Application.UseCases.Articles.CQRS.Commands.Update;
using Application.UseCases.Articles.CQRS.Commands.Delete;
using Application.UseCases.Articles.CQRS.Queries.GetById;
using Application.UseCases.Articles.CQRS.Queries.GetAll;

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

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await mediator.Send(new ArticleGetAllQuery());
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ArticleUpdateCommand command)
    {
        var result = await mediator.Send(command with { Id = id });
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new ArticleDeleteCommand(id));
        return HandleResult(result);
    }
}
