namespace Domain.Entities.Chat;

/// <summary>
/// Mensajes de dominio para Chat en español.
/// </summary>
public static class ChatMessages
{
    public static class Conversation
    {
        public const string NotFound = "Conversación no encontrada";
        public const string Created = "Conversación creada exitosamente";
        public const string UserNotParticipant = "El usuario no es participante de esta conversación";
        public const string Unauthorized = "No tienes permisos para acceder a esta conversación";

        public static string WithId(Guid id) => $"Conversación con ID {id} no encontrada";
        public static string BetweenUsers(Guid user1Id, Guid user2Id) =>
            $"No existe conversación entre los usuarios {user1Id} y {user2Id}";
    }

    public static class Message
    {
        public const string NotFound = "Mensaje no encontrado";
        public const string Sent = "Mensaje enviado exitosamente";
        public const string MarkedAsRead = "Mensaje marcado como leído";
        public const string TooLong = "El mensaje excede el límite de 2000 caracteres";
        public const string Empty = "El mensaje no puede estar vacío";

        public static string WithId(Guid id) => $"Mensaje con ID {id} no encontrado";
    }

    public static class Validation
    {
        public const string InvalidUserId = "ID de usuario inválido";
        public const string InvalidConversationId = "ID de conversación inválido";
        public const string UserNotFound = "Usuario no encontrado";
        public const string BothUsersRequired = "Se requieren dos usuarios para crear una conversación";
        public const string SameUser = "No se puede crear una conversación con el mismo usuario";
    }
}
