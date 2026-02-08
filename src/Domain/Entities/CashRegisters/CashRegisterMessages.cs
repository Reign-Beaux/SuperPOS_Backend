namespace Domain.Entities.CashRegisters;

public static class CashRegisterMessages
{
    public static class Create
    {
        public const string Success = "Corte de caja registrado exitosamente";
        public const string InvalidDateRange = "La fecha de apertura debe ser anterior a la fecha de cierre";
        public const string FutureDateNotAllowed = "No se pueden registrar cortes con fechas futuras";
        public const string NegativeInitialCash = "El efectivo inicial no puede ser negativo";
        public const string NegativeFinalCash = "El efectivo final no puede ser negativo";
        public const string UserNotFound = "El usuario no existe";
        public const string NoSalesInPeriod = "No se encontraron ventas en el periodo especificado";

        public static string WithDateRange(DateTime openingDate, DateTime closingDate) =>
            $"Corte de caja del {openingDate:dd/MM/yyyy HH:mm} al {closingDate:dd/MM/yyyy HH:mm}";
    }

    public static class NotFound
    {
        public const string Default = "Corte de caja no encontrado";

        public static string WithId(Guid id) =>
            $"Corte de caja no encontrado con ID: {id}";
    }
}
