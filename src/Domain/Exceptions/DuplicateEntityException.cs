namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when attempting to create an entity that violates uniqueness constraints.
/// Use this for domain-level uniqueness violations (not database constraint violations).
/// </summary>
public sealed class DuplicateEntityException : DomainException
{
    public string EntityType { get; }
    public string ConflictingProperty { get; }
    public object? ConflictingValue { get; }

    public DuplicateEntityException(string entityType, string conflictingProperty, object? conflictingValue, string? customMessage = null)
        : base("DUPLICATE_ENTITY", customMessage ?? $"{entityType} with {conflictingProperty} '{conflictingValue}' already exists.")
    {
        EntityType = entityType;
        ConflictingProperty = conflictingProperty;
        ConflictingValue = conflictingValue;
    }
}
