namespace Domain.Entities.Inventories;

public static class InventoryMessages
{
    public static class AdjustStock
    {
        public const string Success = "Stock ajustado exitosamente";
        public const string ProductNotFound = "El producto no existe";
        public const string InvalidQuantity = "La cantidad resultante no puede ser negativa";
    }

    public static class NotFound
    {
        public const string ByProductId = "No se encontró inventario para el producto especificado";
        public const string Default = "Inventario no encontrado";

        public static string WithProductId(Guid productId) =>
            $"No se encontró inventario para el producto con ID: {productId}";
    }

    public static class Validation
    {
        public const string ProductIdRequired = "El ID del producto es requerido";
        public const string QuantityRequired = "La cantidad es requerida";
        public const string NegativeQuantityNotAllowed = "La cantidad no puede ser negativa";
    }
}
