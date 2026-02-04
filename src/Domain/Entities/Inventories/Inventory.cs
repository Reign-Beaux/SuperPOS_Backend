using Domain.Entities.Products;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities.Inventories;

/// <summary>
/// Inventory aggregate root managing stock levels for products.
/// </summary>
public class Inventory : BaseEntity, IAggregateRoot
{
    // Backing field for value object
    private Quantity? _quantity;

    // Parameterless constructor required by EF Core
    public Inventory() { }

    // Primitive properties for EF Core persistence
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }

    // Value Object property for domain logic
    public Quantity QuantityValue
    {
        get => _quantity ??= ValueObjects.Quantity.Create(Quantity);
        private set => _quantity = value;
    }

    // Navigation Properties
    public Product Product { get; set; } = null!;

    /// <summary>
    /// Factory method to create a new Inventory with initial stock.
    /// </summary>
    public static Inventory Create(Guid productId, Quantity initialQuantity)
    {
        if (productId == Guid.Empty)
            throw new BusinessRuleViolationException("INVENTORY_001", "Product ID cannot be empty");

        return new Inventory
        {
            ProductId = productId,
            Quantity = initialQuantity.Value,
            _quantity = initialQuantity
        };
    }

    /// <summary>
    /// Adds stock to the inventory.
    /// </summary>
    public void AddStock(Quantity quantity)
    {
        if (quantity.Value <= 0)
            throw new InvalidQuantityException("Cannot add zero or negative stock", quantity.Value);

        var newQuantity = QuantityValue.Add(quantity);
        Quantity = newQuantity.Value;
        _quantity = newQuantity;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds stock by integer amount.
    /// </summary>
    public void AddStock(int amount)
    {
        AddStock(ValueObjects.Quantity.Create(amount));
    }

    /// <summary>
    /// Removes stock from the inventory.
    /// Throws InsufficientStockException if requested amount exceeds available stock.
    /// </summary>
    public void RemoveStock(Quantity quantity)
    {
        if (quantity.Value <= 0)
            throw new InvalidQuantityException("Cannot remove zero or negative stock", quantity.Value);

        if (!HasSufficientStock(quantity))
            throw new InsufficientStockException(Quantity, quantity.Value);

        var newQuantity = QuantityValue.Subtract(quantity);
        Quantity = newQuantity.Value;
        _quantity = newQuantity;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes stock by integer amount.
    /// </summary>
    public void RemoveStock(int amount)
    {
        RemoveStock(ValueObjects.Quantity.Create(amount));
    }

    /// <summary>
    /// Sets the inventory quantity to a specific value.
    /// Used for stock adjustments and corrections.
    /// </summary>
    public void SetStock(Quantity quantity)
    {
        if (quantity.Value < 0)
            throw new InvalidQuantityException("Stock quantity cannot be negative", quantity.Value);

        Quantity = quantity.Value;
        _quantity = quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the inventory quantity by integer amount.
    /// </summary>
    public void SetStock(int amount)
    {
        SetStock(ValueObjects.Quantity.Create(amount));
    }

    /// <summary>
    /// Checks if there is sufficient stock to fulfill a required quantity.
    /// </summary>
    public bool HasSufficientStock(Quantity required) => QuantityValue.Value >= required.Value;

    /// <summary>
    /// Checks if there is sufficient stock by integer amount.
    /// </summary>
    public bool HasSufficientStock(int required) => Quantity >= required;

    /// <summary>
    /// Checks if the inventory is completely out of stock.
    /// </summary>
    public bool IsOutOfStock() => Quantity == 0;

    /// <summary>
    /// Checks if the inventory is running low on stock.
    /// Default threshold is 10 units.
    /// </summary>
    public bool IsLowStock(int threshold = 10) => Quantity > 0 && Quantity <= threshold;

    /// <summary>
    /// Gets the available quantity as a Quantity value object.
    /// </summary>
    public Quantity GetAvailableQuantity() => QuantityValue;

    /// <summary>
    /// Calculates the percentage of stock remaining based on a maximum capacity.
    /// </summary>
    public decimal GetStockPercentage(int maxCapacity)
    {
        if (maxCapacity <= 0)
            throw new InvalidQuantityException("Max capacity must be positive", maxCapacity);

        return (decimal)Quantity / maxCapacity * 100;
    }
}
