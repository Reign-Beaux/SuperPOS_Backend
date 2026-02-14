using System.Text.RegularExpressions;
using Domain.Exceptions;

namespace Domain.ValueObjects;

/// <summary>
/// Represents a valid password with complexity requirements.
/// Immutable value object that ensures password meets security standards.
/// </summary>
/// <remarks>
/// Password requirements (homologated with frontend):
/// - Minimum 8 characters
/// - Maximum 32 characters
/// - At least one uppercase letter
/// - At least one lowercase letter
/// - At least one number
/// - At least one special character ($, %, &, @)
/// </remarks>
public sealed partial class Password : ValueObject
{
    private const int MinLength = 8;
    private const int MaxLength = 32;
    private const string AllowedSpecialChars = "$%&@";

    private static readonly Regex UppercaseRegex = GenerateUppercaseRegex();
    private static readonly Regex LowercaseRegex = GenerateLowercaseRegex();
    private static readonly Regex DigitRegex = GenerateDigitRegex();
    private static readonly Regex SpecialCharRegex = GenerateSpecialCharRegex();

    public string Value { get; }

    private Password(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a Password instance after validating complexity requirements.
    /// </summary>
    /// <param name="password">The plain text password to validate</param>
    /// <returns>A valid Password instance</returns>
    /// <exception cref="InvalidValueObjectException">When password doesn't meet complexity requirements</exception>
    public static Password Create(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidValueObjectException(
                nameof(Password),
                "La contraseña no puede estar vacía.",
                password);

        // Trim para evitar espacios al inicio/final pero mantener espacios internos si existen
        var trimmedPassword = password.Trim();

        // Validar longitud mínima
        if (trimmedPassword.Length < MinLength)
            throw new InvalidValueObjectException(
                nameof(Password),
                $"La contraseña debe tener al menos {MinLength} caracteres.",
                password);

        // Validar longitud máxima
        if (trimmedPassword.Length > MaxLength)
            throw new InvalidValueObjectException(
                nameof(Password),
                $"La contraseña debe tener como máximo {MaxLength} caracteres.",
                password);

        // Validar al menos una mayúscula
        if (!UppercaseRegex.IsMatch(trimmedPassword))
            throw new InvalidValueObjectException(
                nameof(Password),
                "La contraseña debe contener al menos una letra mayúscula.",
                password);

        // Validar al menos una minúscula
        if (!LowercaseRegex.IsMatch(trimmedPassword))
            throw new InvalidValueObjectException(
                nameof(Password),
                "La contraseña debe contener al menos una letra minúscula.",
                password);

        // Validar al menos un número
        if (!DigitRegex.IsMatch(trimmedPassword))
            throw new InvalidValueObjectException(
                nameof(Password),
                "La contraseña debe contener al menos un número.",
                password);

        // Validar al menos un carácter especial ($, %, &, @)
        if (!SpecialCharRegex.IsMatch(trimmedPassword))
            throw new InvalidValueObjectException(
                nameof(Password),
                $"La contraseña debe contener al menos un carácter especial ({AllowedSpecialChars}).",
                password);

        return new Password(trimmedPassword);
    }

    public override string ToString() => "********"; // Never expose password value in logs

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    // Implicit conversion from Password to string (for internal use only)
    public static implicit operator string(Password password) => password.Value;

    // Explicit conversion to Password
    public static explicit operator Password(string password) => Create(password);

    // Regex patterns
    [GeneratedRegex(@"[A-Z]", RegexOptions.Compiled)]
    private static partial Regex GenerateUppercaseRegex();

    [GeneratedRegex(@"[a-z]", RegexOptions.Compiled)]
    private static partial Regex GenerateLowercaseRegex();

    [GeneratedRegex(@"[0-9]", RegexOptions.Compiled)]
    private static partial Regex GenerateDigitRegex();

    [GeneratedRegex(@"[$%&@]", RegexOptions.Compiled)]
    private static partial Regex GenerateSpecialCharRegex();
}
