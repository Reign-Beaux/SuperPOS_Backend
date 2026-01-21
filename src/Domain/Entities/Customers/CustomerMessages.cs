namespace Domain.Entities.Customers;

public static class CustomerMessages
{
    public static class Create
    {
        public const string Success = "El cliente se ha creado correctamente.";
        public const string Failed = "No se pudo insertar el cliente.";
    }

    public static class Update
    {
        public const string Success = "El cliente se ha actualizado correctamente.";
        public const string Failed = "No se pudo actualizar el cliente.";
    }

    public static class Delete
    {
        public const string Success = "El cliente se ha eliminado correctamente.";
        public const string Failed = "No se pudo eliminar el cliente.";
        public const string NotAllowed = "El cliente no se puede eliminar porque tiene movimientos asociados.";
    }

    public static class NotFound
    {
        public const string General = "El cliente no existe.";
        public static string WithId(string value) => string.Format("El cliente con Id {0} no existe.", value);
        public const string WithEmail = "El cliente con email '{0}' no existe.";
    }

    public static class AlreadyExists
    {
        public static string WithEmail(string value) => string.Format("Ya existe un cliente con el email '{0}'.", value);
    }
}
