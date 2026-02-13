using Application.DesignPatterns.Mediators.Interfaces;
using Application.UseCases.Users.CQRS.Commands.Create;
using Application.UseCases.Users.CQRS.Commands.Update;
using Application.UseCases.Users.CQRS.Commands.Delete;
using Application.UseCases.Users.CQRS.Queries.GetById;
using Application.UseCases.Users.CQRS.Queries.GetAll;
using Application.UseCases.Users.CQRS.Queries.Search;
using Microsoft.AspNetCore.Authorization;

namespace Web.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class UserController(IMediator mediator) : BaseController
{
    [HttpPost]
    [AllowAnonymous] // TEMPORAL: Permitir crear el primer usuario admin sin autenticación
    // [Authorize(Policy = "AdminOnly")] // DESCOMENTAR después de crear el usuario admin
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command)
    {
        var result = await mediator.Send(command);
        return HandleResult(result, nameof(GetById));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "ManagementOnly")]
    public async Task<IActionResult> GetById(Guid id)
    {
        UserGetByIdQuery request = new(id);
        var result = await mediator.Send(request);
        return HandleResult(result);
    }

    [HttpGet]
    [Authorize(Policy = "ManagementOnly")]
    public async Task<IActionResult> GetAll()
    {
        var result = await mediator.Send(new UserGetAllQuery());
        return HandleResult(result);
    }

    [HttpGet("search")]
    [Authorize(Policy = "ManagementOnly")]
    public async Task<IActionResult> Search([FromQuery] string term)
    {
        var result = await mediator.Send(new UserSearchQuery(term));
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UserUpdateCommand command)
    {
        if (id != command.Id)
            return BadRequest("Id mismatch");

        var result = await mediator.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new UserDeleteCommand(id));
        return HandleResult(result);
    }
}
