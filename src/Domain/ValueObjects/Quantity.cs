using Domain.Exceptions;

namespace Domain.ValueObjects;

/// <summary>
/// Represents a non-negative quantity of items.
/// Immutable value object that ensures quantities are always valid.
/// </summary>
public sealed class Quantity : ValueObject
{
    public int Value { get; }

    private Quantity(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a Quantity instance. Value must be non-negative.
    /// </summary>
    public static Quantity Create(int value)
    {
        if (value < 0)
            throw new InvalidQuantityException(value);

        return new Quantity(value);
    }

    /// <summary>
    /// Creates a zero Quantity instance.
    /// </summary>
    public static Quantity Zero() => new(0);

    /// <summary>
    /// Creates a Quantity of one.
    /// </summary>
    public static Quantity One() => new(1);

    /// <summary>
    /// Adds two quantities together.
    /// </summary>
    public Quantity Add(Quantity other)
    {
        return new Quantity(Value + other.Value);
    }

    /// <summary>
    /// Adds an integer value to this quantity.
    /// </summary>
    public Quantity Add(int value)
    {
        if (value < 0)
            throw new InvalidQuantityException("Cannot add negative value to quantity.", value);

        return new Quantity(Value + value);
    }

    /// <summary>
    /// Subtracts another quantity from this one.
    /// Result must be non-negative.
    /// </summary>
    public Quantity Subtract(Quantity other)
    {
        var result = Value - other.Value;

        if (result < 0)
            throw new InvalidQuantityException($"Subtraction would result in negative quantity. Current: {Value}, Subtracting: {other.Value}", result);

        return new Quantity(result);
    }

    /// <summary>
    /// Subtracts an integer value from this quantity.
    /// Result must be non-negative.
    /// </summary>
    public Quantity Subtract(int value)
    {
        if (value < 0)
            throw new InvalidQuantityException("Cannot subtract negative value from quantity.", value);

        var result = Value - value;

        if (result < 0)
            throw new InvalidQuantityException($"Subtraction would result in negative quantity. Current: {Value}, Subtracting: {value}", result);

        return new Quantity(result);
    }

    /// <summary>
    /// Checks if this quantity is sufficient to fulfill a required quantity.
    /// </summary>
    public bool IsAvailable(Quantity required) => Value >= required.Value;

    /// <summary>
    /// Checks if this quantity is sufficient to fulfill a required amount.
    /// </summary>
    public bool IsAvailable(int required) => Value >= required;

    /// <summary>
    /// Checks if quantity is zero.
    /// </summary>
    public bool IsZero() => Value == 0;

    /// <summary>
    /// Checks if quantity is positive (greater than zero).
    /// </summary>
    public bool IsPositive() => Value > 0;

    public override string ToString() => Value.ToString();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    // Implicit conversion from int
    public static implicit operator Quantity(int value) => Create(value);

    // Explicit conversion to int
    public static explicit operator int(Quantity quantity) => quantity.Value;

    // Arithmetic operators
    public static Quantity operator +(Quantity left, Quantity right) => left.Add(right);
    public static Quantity operator +(Quantity left, int right) => left.Add(right);
    public static Quantity operator -(Quantity left, Quantity right) => left.Subtract(right);
    public static Quantity operator -(Quantity left, int right) => left.Subtract(right);

    // Comparison operators
    public static bool operator >(Quantity left, Quantity right) => left.Value > right.Value;
    public static bool operator <(Quantity left, Quantity right) => left.Value < right.Value;
    public static bool operator >=(Quantity left, Quantity right) => left.Value >= right.Value;
    public static bool operator <=(Quantity left, Quantity right) => left.Value <= right.Value;
    public static bool operator >(Quantity left, int right) => left.Value > right;
    public static bool operator <(Quantity left, int right) => left.Value < right;
    public static bool operator >=(Quantity left, int right) => left.Value >= right;
    public static bool operator <=(Quantity left, int right) => left.Value <= right;
}
