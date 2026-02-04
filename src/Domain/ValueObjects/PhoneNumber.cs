using System.Text.RegularExpressions;
using Domain.Exceptions;

namespace Domain.ValueObjects;

/// <summary>
/// Represents a valid phone number.
/// Immutable value object that ensures phone numbers contain only valid characters.
/// </summary>
public sealed partial class PhoneNumber : ValueObject
{
    private static readonly Regex PhoneRegex = GeneratePhoneRegex();

    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a PhoneNumber instance after validating the format.
    /// Accepts numbers with optional country code, spaces, dashes, parentheses.
    /// </summary>
    public static PhoneNumber Create(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new InvalidValueObjectException(nameof(PhoneNumber), "Phone number cannot be empty.", phoneNumber);

        var normalized = phoneNumber.Trim();

        if (!PhoneRegex.IsMatch(normalized))
            throw new InvalidValueObjectException(nameof(PhoneNumber), $"Invalid phone number format: {phoneNumber}", phoneNumber);

        // Extract only digits for length validation
        var digitsOnly = Regex.Replace(normalized, @"[^\d]", "");

        if (digitsOnly.Length < 10)
            throw new InvalidValueObjectException(nameof(PhoneNumber), "Phone number must contain at least 10 digits.", phoneNumber);

        return new PhoneNumber(normalized);
    }

    /// <summary>
    /// Returns the phone number with only digits (removes formatting).
    /// </summary>
    public string GetDigitsOnly() => Regex.Replace(Value, @"[^\d]", "");

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        // Compare by digits only to handle different formatting of same number
        yield return GetDigitsOnly();
    }

    // Implicit conversion from string
    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;

    // Explicit conversion to PhoneNumber
    public static explicit operator PhoneNumber(string phoneNumber) => Create(phoneNumber);

    // Phone validation regex - accepts various formats
    [GeneratedRegex(@"^[\+]?[(]?[0-9]{1,4}[)]?[-\s\.]?[(]?[0-9]{1,4}[)]?[-\s\.]?[0-9]{1,9}$", RegexOptions.Compiled)]
    private static partial Regex GeneratePhoneRegex();
}
