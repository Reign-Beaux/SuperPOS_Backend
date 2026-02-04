namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when attempting to create a value object with invalid data.
/// Use this for validation failures during value object construction.
/// </summary>
public sealed class InvalidValueObjectException : DomainException
{
    public string ValueObjectType { get; }
    public object? AttemptedValue { get; }

    public InvalidValueObjectException(string valueObjectType, string message, object? attemptedValue = null)
        : base($"INVALID_{valueObjectType.ToUpperInvariant()}", message)
    {
        ValueObjectType = valueObjectType;
        AttemptedValue = attemptedValue;
    }

    public InvalidValueObjectException(string valueObjectType, string message, Exception innerException, object? attemptedValue = null)
        : base($"INVALID_{valueObjectType.ToUpperInvariant()}", message, innerException)
    {
        ValueObjectType = valueObjectType;
        AttemptedValue = attemptedValue;
    }
}
