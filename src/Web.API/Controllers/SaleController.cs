using Application.DesignPatterns.Mediators.Interfaces;
using Application.Interfaces.Services;
using Application.UseCases.Sales.CQRS.Commands.Cancel;
using Application.UseCases.Sales.CQRS.Commands.Create;
using Application.UseCases.Sales.CQRS.Queries.GetAll;
using Application.UseCases.Sales.CQRS.Queries.GetById;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Controllers;

[Route("api/[controller]")]
public class SaleController : BaseController
{
    private readonly IMediator _mediator;
    private readonly ITicketService _ticketService;

    public SaleController(IMediator mediator, ITicketService ticketService)
    {
        _mediator = mediator;
        _ticketService = ticketService;
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

    /// <summary>
    /// Generate PDF ticket for a sale
    /// </summary>
    /// <param name="id">Sale ID</param>
    /// <returns>PDF file</returns>
    [HttpGet("{id:guid}/ticket")]
    public async Task<IActionResult> GetTicket(Guid id)
    {
        try
        {
            var pdfBytes = await _ticketService.GenerateSaleTicketAsync(id);
            var fileName = $"Ticket-{id.ToString().Substring(0, 8).ToUpper()}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error generating ticket", detail = ex.Message });
        }
    }

    /// <summary>
    /// Cancel a sale and restore inventory
    /// </summary>
    /// <param name="id">Sale ID</param>
    /// <param name="request">Cancellation request with user ID and reason</param>
    /// <returns>Result of cancellation</returns>
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelSale(Guid id, [FromBody] CancelSaleRequest request)
    {
        var command = new CancelSaleCommand(id, request.UserId, request.Reason);
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }
}

/// <summary>
/// Request model for cancelling a sale
/// </summary>
public record CancelSaleRequest(Guid UserId, string Reason);
