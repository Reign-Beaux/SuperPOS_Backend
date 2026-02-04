namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when attempting to remove more stock than available.
/// Specific to inventory/stock management operations.
/// </summary>
public sealed class InsufficientStockException : DomainException
{
    public int Available { get; }
    public int Required { get; }

    public InsufficientStockException(string message, int available = 0, int required = 0)
        : base("INSUFFICIENT_STOCK", message)
    {
        Available = available;
        Required = required;
    }

    public InsufficientStockException(int available, int required)
        : base("INSUFFICIENT_STOCK", $"Insufficient stock. Available: {available}, Required: {required}")
    {
        Available = available;
        Required = required;
    }
}
