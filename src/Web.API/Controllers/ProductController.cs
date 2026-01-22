using Application.DesignPatterns.Mediators.Interfaces;
using Application.UseCases.Products.CQRS.Commands.Create;
using Application.UseCases.Products.CQRS.Commands.Update;
using Application.UseCases.Products.CQRS.Commands.Delete;
using Application.UseCases.Products.CQRS.Queries.GetById;
using Application.UseCases.Products.CQRS.Queries.GetAll;

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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        ProductGetByIdQuery request = new(id);
        var result = await mediator.Send(request);
        return HandleResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await mediator.Send(new ProductGetAllQuery());
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductUpdateCommand command)
    {
        var result = await mediator.Send(command with { Id = id });
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new ProductDeleteCommand(id));
        return HandleResult(result);
    }
}
