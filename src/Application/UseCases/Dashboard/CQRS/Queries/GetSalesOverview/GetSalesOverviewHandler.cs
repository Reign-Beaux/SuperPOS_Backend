using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Dashboard.CQRS.Queries.GetDailySummary;
using Application.UseCases.Dashboard.CQRS.Queries.GetHourlyTrends;
using Application.UseCases.Dashboard.CQRS.Queries.GetTopCustomers;
using Application.UseCases.Dashboard.CQRS.Queries.GetTopProducts;
using Application.UseCases.Dashboard.DTOs;

namespace Application.UseCases.Dashboard.CQRS.Queries.GetSalesOverview;

public class GetSalesOverviewHandler
    : IRequestHandler<GetSalesOverviewQuery, OperationResult<SalesOverviewDTO>>
{
    private readonly IMediator _mediator;

    public GetSalesOverviewHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<OperationResult<SalesOverviewDTO>> Handle(
        GetSalesOverviewQuery request,
        CancellationToken cancellationToken)
    {
        // Ejecutar múltiples consultas en paralelo para máximo rendimiento
        var todayTask = _mediator.Send(
            new GetDailySummaryQuery(DashboardPeriod.Today),
            cancellationToken);

        var weekTask = _mediator.Send(
            new GetDailySummaryQuery(DashboardPeriod.ThisWeek),
            cancellationToken);

        var monthTask = _mediator.Send(
            new GetDailySummaryQuery(DashboardPeriod.ThisMonth),
            cancellationToken);

        var topProductsTask = _mediator.Send(
            new GetTopProductsQuery(DashboardPeriod.ThisMonth, TopCount: 5),
            cancellationToken);

        var topCustomersTask = _mediator.Send(
            new GetTopCustomersQuery(DashboardPeriod.ThisMonth, TopCount: 5),
            cancellationToken);

        var hourlyTask = _mediator.Send(
            new GetHourlyTrendsQuery(DashboardPeriod.Today),
            cancellationToken);

        // Esperar a que todas las consultas terminen
        await Task.WhenAll(todayTask, weekTask, monthTask, topProductsTask, topCustomersTask, hourlyTask);

        // Verificar que todas las consultas fueron exitosas
        if (!todayTask.Result.IsSuccess ||
            !weekTask.Result.IsSuccess ||
            !monthTask.Result.IsSuccess ||
            !topProductsTask.Result.IsSuccess ||
            !topCustomersTask.Result.IsSuccess ||
            !hourlyTask.Result.IsSuccess)
        {
            return Result.Error(
                ErrorResult.InternalServerError,
                detail: "Error al cargar el resumen del dashboard. Una o más consultas fallaron.");
        }

        // Crear el DTO de vista general
        var overview = new SalesOverviewDTO(
            TodaySummary: todayTask.Result.Value!,
            ThisWeekSummary: weekTask.Result.Value!,
            ThisMonthSummary: monthTask.Result.Value!,
            TopProducts: topProductsTask.Result.Value!,
            TopCustomers: topCustomersTask.Result.Value!,
            HourlyTrends: hourlyTask.Result.Value!
        );

        return Result.Success(overview);
    }
}
