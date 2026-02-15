using Application.DesignPatterns.Mediators.Interfaces;
using Application.UseCases.Dashboard.CQRS.Queries.GetDailySummary;
using Application.UseCases.Dashboard.CQRS.Queries.GetHourlyTrends;
using Application.UseCases.Dashboard.CQRS.Queries.GetPeriodComparison;
using Application.UseCases.Dashboard.CQRS.Queries.GetSalesOverview;
using Application.UseCases.Dashboard.CQRS.Queries.GetTopCustomers;
using Application.UseCases.Dashboard.CQRS.Queries.GetTopProducts;
using Application.UseCases.Dashboard.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Web.API.Controllers;

/// <summary>
/// Controlador de Dashboard y Analíticas para inteligencia de negocios.
/// Proporciona datos agregados de ventas, tendencias y métricas de rendimiento.
/// </summary>
[Route("api/[controller]")]
[Authorize(Policy = "ManagerOrAbove")]  // Solo Gerentes y Administradores
public class DashboardController : BaseController
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene una vista general completa del dashboard con resúmenes de hoy, semana y mes.
    /// Incluye top productos, top clientes y tendencias horarias.
    /// </summary>
    /// <returns>Vista general completa del dashboard</returns>
    /// <response code="200">Retorna la vista general del dashboard</response>
    /// <response code="401">Usuario no autenticado</response>
    /// <response code="403">Usuario no autorizado (requiere rol Gerente o Administrador)</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("overview")]
    [ProducesResponseType(typeof(SalesOverviewDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOverview()
    {
        var query = new GetSalesOverviewQuery();
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Obtiene el resumen de ventas para un período específico.
    /// Soporta períodos predefinidos (Today, ThisWeek, ThisMonth) o rangos personalizados.
    /// </summary>
    /// <param name="period">Período predefinido (Today, ThisWeek, ThisMonth, etc.)</param>
    /// <param name="startDate">Fecha de inicio personalizada (requerido si period = Custom)</param>
    /// <param name="endDate">Fecha de fin personalizada (requerido si period = Custom)</param>
    /// <returns>Estadísticas de resumen de ventas</returns>
    /// <response code="200">Retorna el resumen de ventas</response>
    /// <response code="400">Parámetros de consulta inválidos</response>
    /// <response code="401">Usuario no autenticado</response>
    /// <response code="403">Usuario no autorizado</response>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DailySummaryDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DashboardPeriod period = DashboardPeriod.Today,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var query = new GetDailySummaryQuery(period, startDate, endDate);
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Compara el período actual con el período anterior.
    /// Muestra porcentajes de cambio en ingresos, número de ventas y tamaño promedio del ticket.
    /// </summary>
    /// <param name="period">Período a comparar (Today, ThisWeek, ThisMonth - NO soporta Custom)</param>
    /// <returns>Comparación del período con cambios porcentuales</returns>
    /// <response code="200">Retorna la comparación de períodos</response>
    /// <response code="400">Período inválido o no soportado para comparación</response>
    /// <response code="401">Usuario no autenticado</response>
    /// <response code="403">Usuario no autorizado</response>
    [HttpGet("comparison")]
    [ProducesResponseType(typeof(PeriodComparisonDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetComparison(
        [FromQuery] DashboardPeriod period = DashboardPeriod.ThisMonth)
    {
        var query = new GetPeriodComparisonQuery(period);
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Obtiene los productos más vendidos ordenados por cantidad vendida.
    /// </summary>
    /// <param name="period">Período de tiempo para el análisis</param>
    /// <param name="startDate">Fecha de inicio personalizada</param>
    /// <param name="endDate">Fecha de fin personalizada</param>
    /// <param name="top">Número de productos a retornar (1-50, predeterminado: 10)</param>
    /// <returns>Lista de productos más vendidos con métricas de ventas</returns>
    /// <response code="200">Retorna la lista de productos más vendidos</response>
    /// <response code="400">Parámetro 'top' fuera de rango (1-50)</response>
    /// <response code="401">Usuario no autenticado</response>
    /// <response code="403">Usuario no autorizado</response>
    [HttpGet("top-products")]
    [ProducesResponseType(typeof(List<TopProductDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTopProducts(
        [FromQuery] DashboardPeriod period = DashboardPeriod.Today,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int top = 10)
    {
        var query = new GetTopProductsQuery(period, startDate, endDate, top);
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Obtiene los clientes más frecuentes ordenados por total gastado.
    /// </summary>
    /// <param name="period">Período de tiempo para el análisis</param>
    /// <param name="startDate">Fecha de inicio personalizada</param>
    /// <param name="endDate">Fecha de fin personalizada</param>
    /// <param name="top">Número de clientes a retornar (1-50, predeterminado: 10)</param>
    /// <returns>Lista de clientes frecuentes con métricas de compras</returns>
    /// <response code="200">Retorna la lista de clientes frecuentes</response>
    /// <response code="400">Parámetro 'top' fuera de rango (1-50)</response>
    /// <response code="401">Usuario no autenticado</response>
    /// <response code="403">Usuario no autorizado</response>
    [HttpGet("top-customers")]
    [ProducesResponseType(typeof(List<TopCustomerDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTopCustomers(
        [FromQuery] DashboardPeriod period = DashboardPeriod.ThisMonth,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int top = 10)
    {
        var query = new GetTopCustomersQuery(period, startDate, endDate, top);
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Obtiene las tendencias de ventas por hora del día mostrando la distribución de ventas a lo largo del día.
    /// </summary>
    /// <param name="period">Período de tiempo para el análisis (recomendado: Today o ThisWeek)</param>
    /// <param name="startDate">Fecha de inicio personalizada</param>
    /// <param name="endDate">Fecha de fin personalizada</param>
    /// <returns>Lista de datos de ventas por hora (0-23 horas)</returns>
    /// <response code="200">Retorna las tendencias de ventas por hora</response>
    /// <response code="401">Usuario no autenticado</response>
    /// <response code="403">Usuario no autorizado</response>
    [HttpGet("hourly-trends")]
    [ProducesResponseType(typeof(List<HourlySaleDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetHourlyTrends(
        [FromQuery] DashboardPeriod period = DashboardPeriod.Today,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var query = new GetHourlyTrendsQuery(period, startDate, endDate);
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }
}
