using Application.DesignPatterns.Mediators.Interfaces;
using Application.UseCases.Inventories.CQRS.Commands.AdjustStock;
using Application.UseCases.Inventories.CQRS.Queries.GetAll;
using Application.UseCases.Inventories.CQRS.Queries.GetByProductId;

namespace Web.API.Controllers;

[Route("api/[controller]")]
public class InventoryController : BaseController
{
    private readonly IMediator _mediator;

    public InventoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("adjust")]
    public async Task<IActionResult> AdjustStock([FromBody] InventoryAdjustStockCommand command)
    {
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet("product/{productId:guid}")]
    public async Task<IActionResult> GetByProductId(Guid productId)
    {
        var query = new InventoryGetByProductIdQuery(productId);
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new InventoryGetAllQuery();
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }
}
