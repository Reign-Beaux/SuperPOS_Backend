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
        public static string WithId(string? value) => string.Format("El usuario con Id {0} no existe.", value ?? "desconocido");
        public const string WithEmail = "El usuario con email '{0}' no existe.";
    }

    public static class AlreadyExists
    {
        public static string WithEmail(string? value) => string.Format("Ya existe un usuario con el email '{0}'.", value ?? "desconocido");
    }

    public static class Authentication
    {
        public const string InvalidCredentials = "Credenciales inválidas. Verifica tu email y contraseña.";
        public const string AccountLocked = "Tu cuenta ha sido bloqueada por múltiples intentos fallidos. Intenta nuevamente en {0} minutos.";
        public const string AccountInactive = "Tu cuenta está inactiva. Contacta al administrador.";
        public const string LoginSuccess = "Inicio de sesión exitoso.";
        public const string LogoutSuccess = "Sesión cerrada correctamente.";
        public const string RefreshTokenInvalid = "Token de actualización inválido o expirado.";
        public const string RefreshTokenRevoked = "El token de actualización ha sido revocado.";
        public const string UnauthorizedAccess = "No tienes permisos para realizar esta acción.";
    }

    public static class PasswordReset
    {
        public const string CodeSentSuccess = "Se ha enviado un código de verificación a tu correo electrónico.";
        public const string CodeSentFailed = "No se pudo enviar el código de verificación. Intenta nuevamente más tarde.";
        public const string CodeVerifiedSuccess = "Código verificado correctamente.";
        public const string CodeInvalid = "El código ingresado es incorrecto.";
        public const string CodeExpired = "El código de verificación ha expirado. Solicita uno nuevo.";
        public const string MaxAttemptsReached = "Has excedido el número máximo de intentos. Solicita un nuevo código.";
        public const string TokenNotFound = "No se encontró un código de verificación válido.";
        public const string TokenAlreadyUsed = "Este código ya ha sido utilizado. Solicita uno nuevo.";
        public const string ResetSuccess = "Tu contraseña ha sido cambiada correctamente.";
        public const string ResetFailed = "No se pudo cambiar tu contraseña. Intenta nuevamente.";
        public const string EmailNotFound = "No se encontró una cuenta con este correo electrónico.";
    }
}
