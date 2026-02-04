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
    // Backing fields for value objects
    private Quantity? _quantity;
    private Money? _unitPrice;
    private Money? _total;

    // Parameterless constructor required by EF Core
    public SaleDetail() { }

    // Primitive properties for EF Core persistence - all with private setters for immutability
    public Guid SaleId { get; internal set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Total { get; private set; }

    // Value Object properties for domain logic
    public Quantity QuantityValue
    {
        get => _quantity ??= ValueObjects.Quantity.Create(Quantity);
        private set => _quantity = value;
    }

    public Money UnitPriceValue
    {
        get => _unitPrice ??= Money.Create(UnitPrice);
        private set => _unitPrice = value;
    }

    public Money TotalValue
    {
        get => _total ??= Money.Create(Total);
        private set => _total = value;
    }

    // Navigation Properties
    public Sale Sale { get; set; } = null!;
    public Product Product { get; set; } = null!;

    /// <summary>
    /// Internal factory method to create a SaleDetail.
    /// Only the Sale aggregate root should call this method.
    /// </summary>
    internal static SaleDetail Create(Guid productId, Quantity quantity, Money unitPrice)
    {
        if (productId == Guid.Empty)
            throw new BusinessRuleViolationException("SALE_DETAIL_001", "Product ID cannot be empty");

        if (quantity.Value <= 0)
            throw new InvalidQuantityException("Quantity must be positive", quantity.Value);

        if (unitPrice.Amount <= 0)
            throw new BusinessRuleViolationException("SALE_DETAIL_002", "Unit price must be positive");

        // Calculate total
        var total = unitPrice.Multiply(quantity.Value);

        return new SaleDetail
        {
            ProductId = productId,
            Quantity = quantity.Value,
            UnitPrice = unitPrice.Amount,
            Total = total.Amount,
            _quantity = quantity,
            _unitPrice = unitPrice,
            _total = total
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
    /// </summary>
    internal void RecalculateTotal()
    {
        var newTotal = UnitPriceValue.Multiply(QuantityValue.Value);
        Total = newTotal.Amount;
        _total = newTotal;
    }

    /// <summary>
    /// Gets the line total as a formatted string.
    /// </summary>
    public string GetFormattedTotal() => TotalValue.ToString();
}
