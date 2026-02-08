using Application.DesignPatterns.Mediators.Interfaces;
using Application.UseCases.CashRegisters.CQRS.Commands.Create;
using Application.UseCases.CashRegisters.CQRS.Queries.GetAll;
using Application.UseCases.CashRegisters.CQRS.Queries.GetById;

namespace Web.API.Controllers;

[Route("api/[controller]")]
public class CashRegisterController : BaseController
{
    private readonly IMediator _mediator;

    public CashRegisterController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Creates a cash register closing and returns detailed report with all sales
    /// </summary>
    /// <param name="command">Cash register closing data</param>
    /// <returns>Created cash register with complete sales report</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCashRegisterCommand command)
    {
        var result = await _mediator.Send(command);
        // Note: No actionName provided because we're returning a complex report, not just the entity
        // The report contains the CashRegister inside, so direct Location header generation doesn't work
        return HandleResult(result);
    }

    /// <summary>
    /// Gets a specific cash register closing by ID
    /// </summary>
    /// <param name="id">Cash register ID</param>
    /// <returns>Cash register details</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new CashRegisterGetByIdQuery(id);
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Gets all cash register closings
    /// </summary>
    /// <returns>List of all cash register closings</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new CashRegisterGetAllQuery();
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }
}
