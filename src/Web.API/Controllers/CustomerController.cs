using Application.DesignPatterns.Mediators.Interfaces;
using Application.UseCases.Customers.CQRS.Commands.Create;
using Application.UseCases.Customers.CQRS.Commands.Update;
using Application.UseCases.Customers.CQRS.Commands.Delete;
using Application.UseCases.Customers.CQRS.Queries.GetById;
using Application.UseCases.Customers.CQRS.Queries.GetAll;

namespace Web.API.Controllers;

[Route("api/[controller]")]
public class CustomerController(IMediator mediator) : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerCommand command)
    {
        var result = await mediator.Send(command);
        return HandleResult(result, nameof(GetById));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        CustomerGetByIdQuery request = new(id);
        var result = await mediator.Send(request);
        return HandleResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await mediator.Send(new CustomerGetAllQuery());
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CustomerUpdateCommand command)
    {
        if (id != command.Id)
            return BadRequest("Id mismatch");

        var result = await mediator.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new CustomerDeleteCommand(id));
        return HandleResult(result);
    }
}
