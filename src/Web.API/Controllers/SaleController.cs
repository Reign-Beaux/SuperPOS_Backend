using Application.DesignPatterns.Mediators.Interfaces;
using Application.UseCases.Sales.CQRS.Commands.Create;
using Application.UseCases.Sales.CQRS.Queries.GetAll;
using Application.UseCases.Sales.CQRS.Queries.GetById;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Controllers;

[Route("api/[controller]")]
public class SaleController : BaseController
{
    private readonly IMediator _mediator;

    public SaleController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSaleCommand command)
    {
        var result = await _mediator.Send(command);
        return HandleResult(result, nameof(GetById));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new SaleGetByIdQuery(id);
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new SaleGetAllQuery();
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }
}
