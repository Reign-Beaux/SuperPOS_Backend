using Application.DesignPatterns.Mediators.Interfaces;
using Application.UseCases.Roles.CQRS.Commands.Create;
using Application.UseCases.Roles.CQRS.Commands.Update;
using Application.UseCases.Roles.CQRS.Commands.Delete;
using Application.UseCases.Roles.CQRS.Queries.GetById;
using Application.UseCases.Roles.CQRS.Queries.GetAll;

namespace Web.API.Controllers;

[Route("api/[controller]")]
public class RoleController(IMediator mediator) : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleCommand command)
    {
        var result = await mediator.Send(command);
        return HandleResult(result, nameof(GetById));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        RoleGetByIdQuery request = new(id);
        var result = await mediator.Send(request);
        return HandleResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await mediator.Send(new RoleGetAllQuery());
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] RoleUpdateCommand command)
    {
        if (id != command.Id)
            return BadRequest("Id mismatch");

        var result = await mediator.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new RoleDeleteCommand(id));
        return HandleResult(result);
    }
}
