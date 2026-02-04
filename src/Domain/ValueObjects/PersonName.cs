using Domain.Exceptions;

namespace Domain.ValueObjects;

/// <summary>
/// Represents a person's full name with first name and last names.
/// Immutable value object that ensures names are non-empty.
/// </summary>
public sealed class PersonName : ValueObject
{
    public string FirstName { get; }
    public string FirstLastname { get; }
    public string? SecondLastname { get; }

    private PersonName(string firstName, string firstLastname, string? secondLastname)
    {
        FirstName = firstName;
        FirstLastname = firstLastname;
        SecondLastname = secondLastname;
    }

    /// <summary>
    /// Creates a PersonName instance after validation.
    /// </summary>
    public static PersonName Create(string firstName, string firstLastname, string? secondLastname = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new InvalidValueObjectException(nameof(PersonName), "First name cannot be empty.", firstName);

        if (string.IsNullOrWhiteSpace(firstLastname))
            throw new InvalidValueObjectException(nameof(PersonName), "First lastname cannot be empty.", firstLastname);

        var normalizedFirstName = firstName.Trim();
        var normalizedFirstLastname = firstLastname.Trim();
        var normalizedSecondLastname = string.IsNullOrWhiteSpace(secondLastname) ? null : secondLastname.Trim();

        return new PersonName(normalizedFirstName, normalizedFirstLastname, normalizedSecondLastname);
    }

    /// <summary>
    /// Returns the full name in the format: FirstName FirstLastname SecondLastname
    /// </summary>
    public string GetFullName()
    {
        return string.IsNullOrWhiteSpace(SecondLastname)
            ? $"{FirstName} {FirstLastname}"
            : $"{FirstName} {FirstLastname} {SecondLastname}";
    }

    /// <summary>
    /// Returns the name in formal format: FirstLastname SecondLastname, FirstName
    /// </summary>
    public string GetFormalName()
    {
        var lastname = string.IsNullOrWhiteSpace(SecondLastname)
            ? FirstLastname
            : $"{FirstLastname} {SecondLastname}";

        return $"{lastname}, {FirstName}";
    }

    public override string ToString() => GetFullName();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FirstName.ToLowerInvariant();
        yield return FirstLastname.ToLowerInvariant();
        yield return SecondLastname?.ToLowerInvariant();
    }
}
