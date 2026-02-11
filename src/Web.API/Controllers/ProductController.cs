using Application.DesignPatterns.Mediators.Interfaces;
using Application.UseCases.Products.CQRS.Commands.Create;
using Application.UseCases.Products.CQRS.Commands.Update;
using Application.UseCases.Products.CQRS.Commands.Delete;
using Application.UseCases.Products.CQRS.Queries.GetById;
using Application.UseCases.Products.CQRS.Queries.GetAll;
using Application.UseCases.Products.CQRS.Queries.Search;
using Microsoft.AspNetCore.Authorization;

namespace Web.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class ProductController(IMediator mediator) : BaseController
{
    [HttpPost]
    [Authorize(Policy = "ManagerOrAbove")]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        var result = await mediator.Send(command);
        return HandleResult(result, nameof(GetById));
    }

    [HttpGet]
    [Authorize(Policy = "SellerOrAbove")]
    public async Task<IActionResult> GetAll()
    {
        var result = await mediator.Send(new ProductGetAllQuery());
        return HandleResult(result);
    }

    [HttpGet("search")]
    [Authorize(Policy = "SellerOrAbove")]
    public async Task<IActionResult> Search([FromQuery] string term)
    {
        var query = new ProductSearchQuery(term);
        var result = await mediator.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "SellerOrAbove")]
    public async Task<IActionResult> GetById(Guid id)
    {
        ProductGetByIdQuery request = new(id);
        var result = await mediator.Send(request);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ManagerOrAbove")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductUpdateCommand command)
    {
        var result = await mediator.Send(command with { Id = id });
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new ProductDeleteCommand(id));
        return HandleResult(result);
    }
}
