using System.Text.RegularExpressions;
using Domain.Exceptions;

namespace Domain.ValueObjects;

/// <summary>
/// Represents a valid email address.
/// Immutable value object that ensures email format is always valid.
/// </summary>
public sealed partial class Email : ValueObject
{
    private static readonly Regex EmailRegex = GenerateEmailRegex();

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates an Email instance after validating the format.
    /// </summary>
    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidValueObjectException(nameof(Email), "Email cannot be empty.", email);

        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(normalizedEmail))
            throw new InvalidValueObjectException(nameof(Email), $"Invalid email format: {email}", email);

        return new Email(normalizedEmail);
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    // Implicit conversion from string
    public static implicit operator string(Email email) => email.Value;

    // Explicit conversion to Email
    public static explicit operator Email(string email) => Create(email);

    // Email validation regex - covers most common cases
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex GenerateEmailRegex();
}
