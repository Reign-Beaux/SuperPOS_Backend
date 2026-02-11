namespace Domain.Entities.Roles;

public static class RoleMessages
{
    public static class Create
    {
        public const string Success = "El rol se ha creado correctamente.";
        public const string Failed = "No se pudo insertar el rol.";
    }

    public static class Update
    {
        public const string Success = "El rol se ha actualizado correctamente.";
        public const string Failed = "No se pudo actualizar el rol.";
    }

    public static class Delete
    {
        public const string Success = "El rol se ha eliminado correctamente.";
        public const string Failed = "No se pudo eliminar el rol.";
        public const string NotAllowed = "El rol no se puede eliminar porque tiene usuarios asociados.";
    }

    public static class NotFound
    {
        public const string General = "El rol no existe.";
        public static string WithId(string? value) => string.Format("El rol con Id {0} no existe.", value ?? "desconocido");
        public const string WithName = "El rol con nombre '{0}' no existe.";
    }

    public static class AlreadyExists
    {
        public static string WithName(string? value) => string.Format("Ya existe un rol con el nombre '{0}'.", value ?? "desconocido");
    }
}
