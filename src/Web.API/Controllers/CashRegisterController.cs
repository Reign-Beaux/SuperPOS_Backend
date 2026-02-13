using Application.DesignPatterns.Mediators.Interfaces;
using Application.Interfaces.Services;
using Application.UseCases.CashRegisters.CQRS.Commands.Create;
using Application.UseCases.CashRegisters.CQRS.Queries.GetAll;
using Application.UseCases.CashRegisters.CQRS.Queries.GetById;
using Microsoft.AspNetCore.Authorization;

namespace Web.API.Controllers;

[Route("api/[controller]")]
[Authorize(Policy = "ManagementOnly")] // Gerente y Admin - NO Vendedor
public class CashRegisterController : BaseController
{
    private readonly IMediator _mediator;
    private readonly ITicketService _ticketService;

    public CashRegisterController(IMediator mediator, ITicketService ticketService)
    {
        _mediator = mediator;
        _ticketService = ticketService;
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

    /// <summary>
    /// Generate PDF report for a cash register closing
    /// </summary>
    /// <param name="id">Cash register ID</param>
    /// <returns>PDF file</returns>
    [HttpGet("{id:guid}/report")]
    public async Task<IActionResult> GetReport(Guid id)
    {
        try
        {
            var pdfBytes = await _ticketService.GenerateCashRegisterReportAsync(id);
            var fileName = $"Corte-Caja-{id.ToString().Substring(0, 8).ToUpper()}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error generating report", detail = ex.Message });
        }
    }
}
