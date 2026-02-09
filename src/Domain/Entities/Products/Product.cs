using Domain.Entities.Sales;
using Domain.Events.Products;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities.Products;

/// <summary>
/// Product aggregate root representing a sellable item in the inventory.
/// </summary>
public class Product : BaseCatalog, IAggregateRoot
{
    // Backing field for value object
    private Barcode? _barcode;

    // Parameterless constructor required by EF Core
    public Product() { }

    // Properties for EF Core persistence
    public string? Barcode { get; set; }
    public decimal UnitPrice { get; set; }

    // Value Object property for domain logic
    public Barcode? BarcodeValue
    {
        get => _barcode ??= Barcode is not null ? ValueObjects.Barcode.Create(Barcode) : null;
        private set => _barcode = value;
    }

    // Navigation Properties
    public ICollection<Inventories.Inventory> Inventories { get; set; } = [];
    public ICollection<SaleDetail> SaleDetails { get; set; } = [];

    /// <summary>
    /// Factory method to create a new Product with valid values.
    /// Enforces business rules at creation time.
    /// Validates that unit price is positive.
    /// </summary>
    public static Product Create(string name, string description, Barcode? barcode, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("PRODUCT_001", "Product name cannot be empty");

        if (name.Length > 200)
            throw new BusinessRuleViolationException("PRODUCT_002", "Product name cannot exceed 200 characters");

        if (unitPrice <= 0)
            throw new BusinessRuleViolationException("PRODUCT_003", "Product price must be positive");

        var product = new Product
        {
            Name = name.Trim(),
            Description = description?.Trim() ?? string.Empty,
            Barcode = barcode?.Value,
            UnitPrice = unitPrice,
            _barcode = barcode
        };

        // Raise domain event
        product.AddDomainEvent(new ProductCreatedEvent(
            product.Id,
            product.Name,
            product.Barcode,
            product.UnitPrice));

        return product;
    }

    /// <summary>
    /// Updates the product price while enforcing business rules.
    /// Validates that new price is positive.
    /// </summary>
    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0)
            throw new BusinessRuleViolationException("PRODUCT_003", "Product price must be positive");

        if (newPrice == UnitPrice)
            return; // No change needed

        var oldPrice = UnitPrice;

        UnitPrice = newPrice;
        UpdatedAt = DateTime.UtcNow;

        // Raise domain event
        AddDomainEvent(new ProductPriceChangedEvent(Id, oldPrice, newPrice));
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
    /// Validates that result is non-negative.
    /// </summary>
    public decimal CalculateTotal(Quantity quantity)
    {
        var total = UnitPrice * quantity.Value;

        if (total < 0)
            throw new BusinessRuleViolationException("PRODUCT_004", "Total cannot be negative");

        return total;
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
