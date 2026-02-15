namespace Application.UseCases.Dashboard.DTOs;

/// <summary>
/// Períodos de tiempo predefinidos para consultas del dashboard.
/// </summary>
public enum DashboardPeriod
{
    Today = 1,
    Yesterday = 2,
    ThisWeek = 3,
    LastWeek = 4,
    ThisMonth = 5,
    LastMonth = 6,
    Custom = 99
}

/// <summary>
/// Clase auxiliar para calcular rangos de fechas desde períodos predefinidos.
/// </summary>
public static class DashboardPeriodHelper
{
    /// <summary>
    /// Obtiene el rango de fechas (Start, End) para un período dado.
    /// </summary>
    /// <param name="period">Período predefinido o Custom</param>
    /// <param name="customStart">Fecha de inicio personalizada (requerido si period = Custom)</param>
    /// <param name="customEnd">Fecha de fin personalizada (requerido si period = Custom)</param>
    /// <returns>Tupla con (fechaInicio, fechaFin) en UTC</returns>
    public static (DateTime Start, DateTime End) GetDateRange(
        DashboardPeriod period,
        DateTime? customStart = null,
        DateTime? customEnd = null)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;

        return period switch
        {
            DashboardPeriod.Today => (today, today.AddDays(1)),
            DashboardPeriod.Yesterday => (today.AddDays(-1), today),
            DashboardPeriod.ThisWeek => (GetStartOfWeek(today), today.AddDays(1)),
            DashboardPeriod.LastWeek => (GetStartOfWeek(today).AddDays(-7), GetStartOfWeek(today)),
            DashboardPeriod.ThisMonth => (new DateTime(today.Year, today.Month, 1), today.AddDays(1)),
            DashboardPeriod.LastMonth => (
                new DateTime(today.Year, today.Month, 1).AddMonths(-1),
                new DateTime(today.Year, today.Month, 1)),
            DashboardPeriod.Custom => (
                customStart?.ToUniversalTime().Date ?? today,
                customEnd?.ToUniversalTime().Date.AddDays(1) ?? today.AddDays(1)),
            _ => (today, today.AddDays(1))
        };
    }

    /// <summary>
    /// Obtiene el inicio de la semana (lunes) para una fecha dada.
    /// </summary>
    private static DateTime GetStartOfWeek(DateTime date)
    {
        // Asumiendo lunes como inicio de semana
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }

    /// <summary>
    /// Obtiene el nombre descriptivo del período en español.
    /// </summary>
    public static string GetPeriodName(DashboardPeriod period)
    {
        return period switch
        {
            DashboardPeriod.Today => "Hoy",
            DashboardPeriod.Yesterday => "Ayer",
            DashboardPeriod.ThisWeek => "Esta semana",
            DashboardPeriod.LastWeek => "Semana anterior",
            DashboardPeriod.ThisMonth => "Este mes",
            DashboardPeriod.LastMonth => "Mes anterior",
            DashboardPeriod.Custom => "Período personalizado",
            _ => "Desconocido"
        };
    }
}
