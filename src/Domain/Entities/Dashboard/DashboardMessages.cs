namespace Domain.Entities.Dashboard;

/// <summary>
/// Mensajes de validación y error para Dashboard & Analytics.
/// Todos los mensajes están en español siguiendo la convención del proyecto.
/// </summary>
public static class DashboardMessages
{
    public const string InvalidTopCount = "El número de registros debe estar entre 1 y 50.";
    public const string InvalidDateRange = "La fecha de inicio debe ser anterior a la fecha de fin.";
    public const string ComparisonNotSupportedForCustom = "La comparación no está soportada para períodos personalizados. Use períodos predefinidos (Today, ThisWeek, ThisMonth).";
    public const string ComparisonFailed = "No se pudo realizar la comparación de períodos.";

    public static class Summary
    {
        public const string NoDataAvailable = "No hay datos disponibles para el período seleccionado.";
        public const string LoadingError = "Error al cargar el resumen de ventas.";
    }

    public static class Products
    {
        public const string NoTopProducts = "No hay productos vendidos en el período seleccionado.";
        public const string LoadingError = "Error al cargar los productos más vendidos.";
    }

    public static class Customers
    {
        public const string NoTopCustomers = "No hay clientes con compras en el período seleccionado.";
        public const string LoadingError = "Error al cargar los clientes frecuentes.";
    }

    public static class Trends
    {
        public const string NoHourlyData = "No hay datos de ventas por hora para el período seleccionado.";
        public const string LoadingError = "Error al cargar las tendencias horarias.";
    }
}
