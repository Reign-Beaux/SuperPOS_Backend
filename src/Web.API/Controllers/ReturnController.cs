using Application.DesignPatterns.Mediators.Interfaces;
using Application.UseCases.Returns.CQRS.Commands.Approve;
using Application.UseCases.Returns.CQRS.Commands.Create;
using Application.UseCases.Returns.CQRS.Commands.Reject;
using Application.UseCases.Returns.CQRS.Queries.GetAll;
using Application.UseCases.Returns.CQRS.Queries.GetById;
using Application.UseCases.Returns.CQRS.Queries.GetByStatus;
using Domain.Entities.Returns;

namespace Web.API.Controllers;

[Route("api/[controller]")]
public class ReturnController : BaseController
{
    private readonly IMediator _mediator;

    public ReturnController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new return request
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReturnCommand command)
    {
        var result = await _mediator.Send(command);
        return HandleResult(result, nameof(GetById));
    }

    /// <summary>
    /// Get return by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new ReturnGetByIdQuery(id);
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all returns
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new ReturnGetAllQuery();
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Get returns by status
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(ReturnStatus status)
    {
        var query = new ReturnGetByStatusQuery(status);
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Approve a return and restore inventory
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveReturnRequest request)
    {
        var command = new ApproveReturnCommand(id, request.ApprovedByUserId);
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Reject a return
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectReturnRequest request)
    {
        var command = new RejectReturnCommand(id, request.RejectedByUserId, request.RejectionReason);
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }
}

/// <summary>
/// Request model for approving a return
/// </summary>
public record ApproveReturnRequest(Guid ApprovedByUserId);

/// <summary>
/// Request model for rejecting a return
/// </summary>
public record RejectReturnRequest(Guid RejectedByUserId, string RejectionReason);
