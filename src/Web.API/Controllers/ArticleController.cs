using Application.DesignPatterns.Mediators.Interfaces;
using Application.UseCases.Articles.CQRS.Queries.GetById;

namespace Web.API.Controllers;

[Route("api/[controller]")]
public class ArticleController(IMediator mediator) : BaseController
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        ArticleGetByIdQuery request = new(id);
        var result = await mediator.Send(request);
        return HandleResult(result);
    }
}
