using Domain.Entities.Sales;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities.Products;

/// <summary>
/// Product aggregate root representing a sellable item in the inventory.
/// </summary>
public class Product : BaseCatalog, IAggregateRoot
{
    // Backing fields for value objects
    private Barcode? _barcode;
    private Money? _unitPrice;

    // Parameterless constructor required by EF Core
    public Product() { }

    // Primitive properties for EF Core persistence
    public string? Barcode { get; set; }
    public decimal UnitPrice { get; set; }

    // Value Object properties for domain logic
    public Barcode? BarcodeValue
    {
        get => _barcode ??= Barcode is not null ? ValueObjects.Barcode.Create(Barcode) : null;
        private set => _barcode = value;
    }

    public Money UnitPriceValue
    {
        get => _unitPrice ??= Money.Create(UnitPrice);
        private set => _unitPrice = value;
    }

    // Navigation Properties
    public ICollection<Inventories.Inventory> Inventories { get; set; } = [];
    public ICollection<SaleDetail> SaleDetails { get; set; } = [];

    /// <summary>
    /// Factory method to create a new Product with valid values.
    /// Enforces business rules at creation time.
    /// </summary>
    public static Product Create(string name, string description, Barcode? barcode, Money unitPrice)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("PRODUCT_001", "Product name cannot be empty");

        if (name.Length > 200)
            throw new BusinessRuleViolationException("PRODUCT_002", "Product name cannot exceed 200 characters");

        if (unitPrice.Amount <= 0)
            throw new BusinessRuleViolationException("PRODUCT_003", "Product price must be positive");

        return new Product
        {
            Name = name.Trim(),
            Description = description?.Trim() ?? string.Empty,
            Barcode = barcode?.Value,
            UnitPrice = unitPrice.Amount,
            _barcode = barcode,
            _unitPrice = unitPrice
        };
    }

    /// <summary>
    /// Updates the product price while enforcing business rules.
    /// </summary>
    public void UpdatePrice(Money newPrice)
    {
        if (newPrice.Amount <= 0)
            throw new BusinessRuleViolationException("PRODUCT_003", "Product price must be positive");

        if (newPrice.Amount == UnitPrice)
            return; // No change needed

        UnitPrice = newPrice.Amount;
        _unitPrice = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the product barcode.
    /// </summary>
    public void UpdateBarcode(Barcode? newBarcode)
    {
        if (newBarcode?.Value == Barcode)
            return; // No change needed

        Barcode = newBarcode?.Value;
        _barcode = newBarcode;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates basic product information.
    /// </summary>
    public void UpdateInfo(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("PRODUCT_001", "Product name cannot be empty");

        if (name.Length > 200)
            throw new BusinessRuleViolationException("PRODUCT_002", "Product name cannot exceed 200 characters");

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Calculates the total price for a given quantity.
    /// </summary>
    public Money CalculateTotal(Quantity quantity)
    {
        return UnitPriceValue.Multiply(quantity.Value);
    }

    /// <summary>
    /// Checks if the product has a barcode assigned.
    /// </summary>
    public bool HasBarcode() => !string.IsNullOrWhiteSpace(Barcode);

    /// <summary>
    /// Gets a formatted display name for the product.
    /// </summary>
    public string GetDisplayName()
    {
        return HasBarcode() ? $"{Name} ({Barcode})" : Name;
    }
}
