namespace Domain.Entities.Returns;

/// <summary>
/// Domain messages for Return entity in Spanish.
/// </summary>
public static class ReturnMessages
{
    public const string NotFound = "Devolución no encontrada";
    public const string Created = "Devolución creada exitosamente";
    public const string Approved = "Devolución aprobada exitosamente";
    public const string Rejected = "Devolución rechazada";

    public static string WithId(Guid id) => $"Devolución con ID {id} no encontrada";

    public static class Create
    {
        public const string SaleNotFound = "Venta no encontrada";
        public const string SaleCancelled = "No se puede crear devolución de una venta cancelada";
        public const string ReturnWindowExpired = "El plazo para devoluciones ha expirado (máximo 30 días)";
        public const string InvalidQuantity = "La cantidad a devolver excede la cantidad comprada";
    }

    public static class Approve
    {
        public const string AlreadyProcessed = "La devolución ya fue procesada";
        public const string OnlyPendingCanBeApproved = "Solo se pueden aprobar devoluciones pendientes";
    }

    public static class Reject
    {
        public const string AlreadyProcessed = "La devolución ya fue procesada";
        public const string OnlyPendingCanBeRejected = "Solo se pueden rechazar devoluciones pendientes";
        public const string ReasonRequired = "Se requiere una razón para rechazar la devolución";
    }
}
