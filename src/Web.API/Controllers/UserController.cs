using Application.DesignPatterns.Mediators.Interfaces;
using Application.UseCases.Users.CQRS.Commands.Create;
using Application.UseCases.Users.CQRS.Commands.Update;
using Application.UseCases.Users.CQRS.Commands.Delete;
using Application.UseCases.Users.CQRS.Queries.GetById;
using Application.UseCases.Users.CQRS.Queries.GetAll;

namespace Web.API.Controllers;

[Route("api/[controller]")]
public class UserController(IMediator mediator) : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command)
    {
        var result = await mediator.Send(command);
        return HandleResult(result, nameof(GetById));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        UserGetByIdQuery request = new(id);
        var result = await mediator.Send(request);
        return HandleResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await mediator.Send(new UserGetAllQuery());
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UserUpdateCommand command)
    {
        if (id != command.Id)
            return BadRequest("Id mismatch");

        var result = await mediator.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new UserDeleteCommand(id));
        return HandleResult(result);
    }
}
