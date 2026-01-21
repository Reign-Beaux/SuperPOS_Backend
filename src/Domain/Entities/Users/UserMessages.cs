namespace Domain.Entities.Users;

public static class UserMessages
{
    public static class Create
    {
        public const string Success = "El usuario se ha creado correctamente.";
        public const string Failed = "No se pudo insertar el usuario.";
    }

    public static class Update
    {
        public const string Success = "El usuario se ha actualizado correctamente.";
        public const string Failed = "No se pudo actualizar el usuario.";
    }

    public static class Delete
    {
        public const string Success = "El usuario se ha eliminado correctamente.";
        public const string Failed = "No se pudo eliminar el usuario.";
        public const string NotAllowed = "El usuario no se puede eliminar porque tiene movimientos asociados.";
    }

    public static class NotFound
    {
        public const string General = "El usuario no existe.";
        public static string WithId(string value) => string.Format("El usuario con Id {0} no existe.", value);
        public const string WithEmail = "El usuario con email '{0}' no existe.";
    }

    public static class AlreadyExists
    {
        public static string WithEmail(string value) => string.Format("Ya existe un usuario con el email '{0}'.", value);
    }
}
