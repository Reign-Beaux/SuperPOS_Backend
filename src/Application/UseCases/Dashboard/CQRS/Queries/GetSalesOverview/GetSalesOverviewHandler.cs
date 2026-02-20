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
        // Ejecutar consultas secuencialmente
        // (EF Core no permite operaciones concurrentes en el mismo DbContext)
        var todayResult = await _mediator.Send(
            new GetDailySummaryQuery(DashboardPeriod.Today),
            cancellationToken);

        var weekResult = await _mediator.Send(
            new GetDailySummaryQuery(DashboardPeriod.ThisWeek),
            cancellationToken);

        var monthResult = await _mediator.Send(
            new GetDailySummaryQuery(DashboardPeriod.ThisMonth),
            cancellationToken);

        var topProductsResult = await _mediator.Send(
            new GetTopProductsQuery(DashboardPeriod.ThisMonth, TopCount: 5),
            cancellationToken);

        var topCustomersResult = await _mediator.Send(
            new GetTopCustomersQuery(DashboardPeriod.ThisMonth, TopCount: 5),
            cancellationToken);

        var hourlyResult = await _mediator.Send(
            new GetHourlyTrendsQuery(DashboardPeriod.Today),
            cancellationToken);

        // Verificar que todas las consultas fueron exitosas
        if (!todayResult.IsSuccess ||
            !weekResult.IsSuccess ||
            !monthResult.IsSuccess ||
            !topProductsResult.IsSuccess ||
            !topCustomersResult.IsSuccess ||
            !hourlyResult.IsSuccess)
        {
            return Result.Error(
                ErrorResult.InternalServerError,
                detail: "Error al cargar el resumen del dashboard. Una o más consultas fallaron.");
        }

        // Crear el DTO de vista general
        var overview = new SalesOverviewDTO(
            TodaySummary: todayResult.Value!,
            ThisWeekSummary: weekResult.Value!,
            ThisMonthSummary: monthResult.Value!,
            TopProducts: topProductsResult.Value!,
            TopCustomers: topCustomersResult.Value!,
            HourlyTrends: hourlyResult.Value!
        );

        return Result.Success(overview);
    }
}
