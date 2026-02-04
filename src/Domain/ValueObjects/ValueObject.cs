namespace Domain.ValueObjects;

/// <summary>
/// Base class for all value objects.
/// Value objects are immutable and compared by their values, not identity.
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Returns the components that define equality for this value object.
    /// Derived classes must implement this to specify which properties determine equality.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Creates a copy of this value object.
    /// Since value objects are immutable, this returns the same instance.
    /// </summary>
    protected ValueObject Copy() => this;
}
