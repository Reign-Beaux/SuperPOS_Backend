namespace Domain.Entities.Products;

public static class ProductMessages
{
    public static class Create
    {
        public const string Success = "El producto se ha creado correctamente.";
        public const string Failed = "No se pudo insertar el producto.";
    }

    public static class Update
    {
        public const string Success = "El producto se ha actualizado correctamente.";
        public const string Failed = "No se pudo actualizar el producto.";
    }

    public static class Delete
    {
        public const string Success = "El producto se ha eliminado correctamente.";
        public const string Failed = "No se pudo eliminar el producto.";
        public const string NotAllowed = "El producto no se puede eliminar porque tiene movimientos asociados.";
    }

    public static class NotFound
    {
        public const string General = "El producto no existe.";
        public static string WithId(string value) => string.Format("El producto con Id {0} no existe.", value);
        public const string WithName = "El producto con nombre '{0}' no existe.";
        public const string WithBarcode = "El producto con código de barras '{0}' no existe.";
    }

    public static class AlreadyExists
    {
        public static string WithName(string value) => string.Format("Ya existe un producto con el nombre '{0}'.", value);
        public static string WithBarcode(string value) => string.Format("Ya existe un producto con el código de barras '{0}'.", value);
    }
}
