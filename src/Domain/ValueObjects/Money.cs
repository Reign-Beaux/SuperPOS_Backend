using Domain.Exceptions;

namespace Domain.ValueObjects;

/// <summary>
/// Represents a monetary amount with currency.
/// Immutable value object that ensures money values are always valid and positive.
/// </summary>
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// Creates a Money instance with the specified amount in USD.
    /// </summary>
    public static Money Create(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new InvalidValueObjectException(nameof(Money), "Money amount cannot be negative.", amount);

        if (string.IsNullOrWhiteSpace(currency))
            throw new InvalidValueObjectException(nameof(Money), "Currency cannot be empty.");

        return new Money(amount, currency);
    }

    /// <summary>
    /// Creates a zero Money instance.
    /// </summary>
    public static Money Zero(string currency = "USD") => new(0, currency);

    /// <summary>
    /// Adds two Money instances. Both must have the same currency.
    /// </summary>
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add money with different currencies: {Currency} and {other.Currency}");

        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Subtracts another Money instance from this one. Both must have the same currency.
    /// </summary>
    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot subtract money with different currencies: {Currency} and {other.Currency}");

        var result = Amount - other.Amount;
        if (result < 0)
            throw new InvalidValueObjectException(nameof(Money), "Subtraction would result in negative amount.", result);

        return new Money(result, Currency);
    }

    /// <summary>
    /// Multiplies the money amount by a factor.
    /// </summary>
    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new InvalidValueObjectException(nameof(Money), "Cannot multiply money by negative factor.", factor);

        return new Money(Amount * factor, Currency);
    }

    /// <summary>
    /// Multiplies the money amount by an integer quantity.
    /// </summary>
    public Money Multiply(int quantity)
    {
        if (quantity < 0)
            throw new InvalidValueObjectException(nameof(Money), "Cannot multiply money by negative quantity.", quantity);

        return new Money(Amount * quantity, Currency);
    }

    /// <summary>
    /// Divides the money amount by a divisor.
    /// </summary>
    public Money Divide(decimal divisor)
    {
        if (divisor <= 0)
            throw new InvalidValueObjectException(nameof(Money), "Cannot divide money by zero or negative divisor.", divisor);

        return new Money(Amount / divisor, Currency);
    }

    /// <summary>
    /// Checks if this money amount is positive (greater than zero).
    /// </summary>
    public bool IsPositive() => Amount > 0;

    /// <summary>
    /// Checks if this money amount is zero.
    /// </summary>
    public bool IsZero() => Amount == 0;

    public override string ToString() => $"{Amount:F2} {Currency}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    // Implicit conversion from decimal (assumes USD)
    public static implicit operator Money(decimal amount) => Create(amount);

    // Explicit conversion to decimal
    public static explicit operator decimal(Money money) => money.Amount;

    // Arithmetic operators
    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money money, decimal factor) => money.Multiply(factor);
    public static Money operator *(Money money, int quantity) => money.Multiply(quantity);
    public static Money operator /(Money money, decimal divisor) => money.Divide(divisor);

    // Comparison operators
    public static bool operator >(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot compare money with different currencies: {left.Currency} and {right.Currency}");
        return left.Amount > right.Amount;
    }

    public static bool operator <(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot compare money with different currencies: {left.Currency} and {right.Currency}");
        return left.Amount < right.Amount;
    }

    public static bool operator >=(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot compare money with different currencies: {left.Currency} and {right.Currency}");
        return left.Amount >= right.Amount;
    }

    public static bool operator <=(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot compare money with different currencies: {left.Currency} and {right.Currency}");
        return left.Amount <= right.Amount;
    }
}
