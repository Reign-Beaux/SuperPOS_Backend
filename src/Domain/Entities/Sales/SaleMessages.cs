namespace Domain.Entities.Sales;

public static class SaleMessages
{
    public static class Create
    {
        public const string Success = "Venta registrada exitosamente";
        public const string CustomerNotFound = "El cliente no existe";
        public const string UserNotFound = "El usuario no existe";
        public const string EmptyItems = "La venta debe incluir al menos un producto";
        public const string InsufficientStock = "Stock insuficiente para el producto";
        public const string ProductNotFound = "Uno o más productos no existen";

        public static string WithProductName(string? productName, int available, int required) =>
            $"Stock insuficiente para '{productName ?? "desconocido"}'. Disponible: {available}, Requerido: {required}";

        public static string ProductNotFoundWithId(Guid productId) =>
            $"Producto no encontrado con ID: {productId}";
    }

    public static class NotFound
    {
        public const string Default = "Venta no encontrada";

        public static string WithId(Guid id) =>
            $"Venta no encontrada con ID: {id}";
    }

    public static class Validation
    {
        public const string CustomerIdRequired = "El ID del cliente es requerido";
        public const string UserIdRequired = "El ID del usuario es requerido";
        public const string ItemsRequired = "Los items son requeridos";
        public const string ProductIdRequired = "El ID del producto es requerido en cada item";
        public const string QuantityMustBePositive = "La cantidad debe ser mayor a cero";
    }
}
