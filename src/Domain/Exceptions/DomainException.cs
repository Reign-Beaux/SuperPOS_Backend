namespace Domain.Exceptions;

/// <summary>
/// Base class for all domain-specific exceptions.
/// Domain exceptions represent business rule violations and invalid states.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// Machine-readable error code for categorization and logging.
    /// </summary>
    public string Code { get; }

    protected DomainException(string code, string message) : base(message)
    {
        Code = code;
    }

    protected DomainException(string code, string message, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }
}
