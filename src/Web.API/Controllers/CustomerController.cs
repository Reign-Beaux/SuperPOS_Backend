using Application.DesignPatterns.Mediators.Interfaces;
using Application.UseCases.Customers.CQRS.Commands.Create;
using Application.UseCases.Customers.CQRS.Commands.Update;
using Application.UseCases.Customers.CQRS.Commands.Delete;
using Application.UseCases.Customers.CQRS.Queries.GetById;
using Application.UseCases.Customers.CQRS.Queries.GetAll;
using Application.UseCases.Customers.CQRS.Queries.Search;
using Microsoft.AspNetCore.Authorization;

namespace Web.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class CustomerController(IMediator mediator) : BaseController
{
    [HttpPost]
    [Authorize(Policy = "ManagementOnly")] // Gerente y Admin
    public async Task<IActionResult> Create([FromBody] CreateCustomerCommand command)
    {
        var result = await mediator.Send(command);
        return HandleResult(result, nameof(GetById));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "ManagementOnly")] // Gerente y Admin
    public async Task<IActionResult> GetById(Guid id)
    {
        CustomerGetByIdQuery request = new(id);
        var result = await mediator.Send(request);
        return HandleResult(result);
    }

    [HttpGet]
    [Authorize(Policy = "ManagementOnly")] // Gerente y Admin
    public async Task<IActionResult> GetAll()
    {
        var result = await mediator.Send(new CustomerGetAllQuery());
        return HandleResult(result);
    }

    [HttpGet("search")]
    [Authorize(Policy = "ManagementOnly")] // Gerente y Admin
    public async Task<IActionResult> Search([FromQuery] string term)
    {
        var result = await mediator.Send(new CustomerSearchQuery(term));
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "ManagementOnly")] // Gerente y Admin
    public async Task<IActionResult> Update(Guid id, [FromBody] CustomerUpdateCommand command)
    {
        if (id != command.Id)
            return BadRequest("Id mismatch");

        var result = await mediator.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")] // Solo Admin
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new CustomerDeleteCommand(id));
        return HandleResult(result);
    }
}
