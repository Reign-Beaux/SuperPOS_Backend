namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when a quantity value is invalid (negative, zero when positive required, etc.).
/// </summary>
public sealed class InvalidQuantityException : DomainException
{
    public int? AttemptedValue { get; }

    public InvalidQuantityException(string message, int? attemptedValue = null)
        : base("INVALID_QUANTITY", message)
    {
        AttemptedValue = attemptedValue;
    }

    public InvalidQuantityException(int attemptedValue)
        : base("INVALID_QUANTITY", $"Invalid quantity: {attemptedValue}. Quantity must be non-negative.")
    {
        AttemptedValue = attemptedValue;
    }
}
