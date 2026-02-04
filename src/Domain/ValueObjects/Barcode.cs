using Domain.Exceptions;

namespace Domain.ValueObjects;

/// <summary>
/// Represents a product barcode.
/// Immutable value object that ensures barcodes are non-empty and alphanumeric.
/// </summary>
public sealed class Barcode : ValueObject
{
    public string Value { get; }

    private Barcode(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a Barcode instance after validation.
    /// </summary>
    public static Barcode Create(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            throw new InvalidValueObjectException(nameof(Barcode), "Barcode cannot be empty.", barcode);

        var normalized = barcode.Trim();

        // Basic validation - allow alphanumeric and common barcode characters
        if (!IsValidBarcodeFormat(normalized))
            throw new InvalidValueObjectException(nameof(Barcode), $"Invalid barcode format: {barcode}. Only alphanumeric characters and hyphens allowed.", barcode);

        return new Barcode(normalized);
    }

    private static bool IsValidBarcodeFormat(string barcode)
    {
        // Allow alphanumeric characters and hyphens
        return barcode.All(c => char.IsLetterOrDigit(c) || c == '-');
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    // Implicit conversion from string
    public static implicit operator string(Barcode barcode) => barcode.Value;

    // Explicit conversion to Barcode
    public static explicit operator Barcode(string barcode) => Create(barcode);
}
