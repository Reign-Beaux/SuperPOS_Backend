using Application.DesignPatterns.Mediators.Interfaces;
using Application.UseCases.Products.CQRS.Commands.Create;
using Application.UseCases.Products.CQRS.Commands.Update;
using Application.UseCases.Products.CQRS.Commands.Delete;
using Application.UseCases.Products.CQRS.Queries.GetById;
using Application.UseCases.Products.CQRS.Queries.GetAll;
using Application.UseCases.Products.CQRS.Queries.Search;

namespace Web.API.Controllers;

[Route("api/[controller]")]
public class ProductController(IMediator mediator) : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        var result = await mediator.Send(command);
        return HandleResult(result, nameof(GetById));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await mediator.Send(new ProductGetAllQuery());
        return HandleResult(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string term)
    {
        var query = new ProductSearchQuery(term);
        var result = await mediator.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        ProductGetByIdQuery request = new(id);
        var result = await mediator.Send(request);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductUpdateCommand command)
    {
        var result = await mediator.Send(command with { Id = id });
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new ProductDeleteCommand(id));
        return HandleResult(result);
    }
}
