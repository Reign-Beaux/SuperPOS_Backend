namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when a business rule or invariant is violated.
/// Use this for domain logic violations that prevent an operation from completing.
/// </summary>
public sealed class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string code, string message)
        : base(code, message)
    {
    }

    public BusinessRuleViolationException(string code, string message, Exception innerException)
        : base(code, message, innerException)
    {
    }
}
