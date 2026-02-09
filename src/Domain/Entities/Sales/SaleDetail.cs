using Domain.Entities.Products;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities.Sales;

/// <summary>
/// SaleDetail is an internal entity of the Sale aggregate.
/// It represents a single line item in a sale and is immutable after creation.
/// Only the Sale aggregate root can create SaleDetail instances.
/// </summary>
public class SaleDetail : BaseEntity
{
    // Backing field for value object
    private Quantity? _quantity;

    // Parameterless constructor required by EF Core
    public SaleDetail() { }

    // Properties for EF Core persistence
    public Guid SaleId { get; set; }  // Public setter required by EF Core for foreign key
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Total { get; private set; }

    // Value Object property for domain logic
    public Quantity QuantityValue
    {
        get => _quantity ??= ValueObjects.Quantity.Create(Quantity);
        private set => _quantity = value;
    }

    // Navigation Properties
    public Sale Sale { get; set; } = null!;
    public Product Product { get; set; } = null!;

    /// <summary>
    /// Internal factory method to create a SaleDetail.
    /// Only the Sale aggregate root should call this method.
    /// Validates that unit price and total are non-negative.
    /// </summary>
    internal static SaleDetail Create(Guid productId, Quantity quantity, decimal unitPrice)
    {
        if (productId == Guid.Empty)
            throw new BusinessRuleViolationException("SALE_DETAIL_001", "Product ID cannot be empty");

        if (quantity.Value <= 0)
            throw new InvalidQuantityException("Quantity must be positive", quantity.Value);

        if (unitPrice < 0)
            throw new InvalidValueObjectException(nameof(unitPrice), "Unit price cannot be negative", unitPrice);

        // Calculate total
        var total = unitPrice * quantity.Value;

        if (total < 0)
            throw new BusinessRuleViolationException("SALE_DETAIL_002", "Total cannot be negative");

        return new SaleDetail
        {
            ProductId = productId,
            Quantity = quantity.Value,
            UnitPrice = unitPrice,
            Total = total,
            _quantity = quantity
        };
    }

    /// <summary>
    /// Internal method to set the SaleId after the Sale is created.
    /// Only the Sale aggregate root should call this.
    /// </summary>
    internal void SetSaleId(Guid saleId)
    {
        if (SaleId != Guid.Empty)
            throw new BusinessRuleViolationException("SALE_DETAIL_003", "SaleId has already been set");

        SaleId = saleId;
    }

    /// <summary>
    /// Recalculates the total based on quantity and unit price.
    /// Used internally to ensure consistency.
    /// Validates that total is non-negative.
    /// </summary>
    internal void RecalculateTotal()
    {
        var newTotal = UnitPrice * QuantityValue.Value;

        if (newTotal < 0)
            throw new BusinessRuleViolationException("SALE_DETAIL_003", "Total cannot be negative");

        Total = newTotal;
    }

    /// <summary>
    /// Gets the line total as a formatted string.
    /// </summary>
    public string GetFormattedTotal() => $"{Total:F2}";
}
