namespace Domain.Entities.Articles;

public static class ArticleMessages
{
    public static class Create
    {
        public const string Success = "El artículo se ha creado correctamente.";
        public const string Failed = "No se pudo insertar el artículo.";
    }

    public static class Update
    {
        public const string Success = "El artículo se ha actualizado correctamente.";
        public const string Failed = "No se pudo actualizar el artículo.";
    }

    public static class Delete
    {
        public const string Success = "El artículo se ha eliminado correctamente.";
        public const string Failed = "No se pudo eliminar el artículo.";
        public const string NotAllowed = "El artículo no se puede eliminar porque tiene movimientos asociados.";
    }

    public static class NotFound
    {
        public const string General = "El artículo no existe.";
        public static string WithId(string value) => string.Format("El artículo con Id {0} no existe.", value);
        public const string WithName = "El artículo con nombre '{0}' no existe.";
        public const string WithBarcode = "El artículo con código de barras '{0}' no existe.";
    }

    public static class AlreadyExists
    {
        public static string WithName(string value) => string.Format("Ya existe un artículo con el nombre '{0}'.", value);
        public static string WithBarcode(string value) => string.Format("Ya existe un artículo con el código de barras '{0}'.", value);
    }
}
