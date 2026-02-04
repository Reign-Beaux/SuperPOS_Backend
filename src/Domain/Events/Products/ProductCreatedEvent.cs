namespace Domain.Events.Products;

/// <summary>
/// Domain event raised when a new product is created.
/// This event can trigger side effects like:
/// - Creating initial inventory record
/// - Notifying catalog management systems
/// - Updating search indexes
/// </summary>
public sealed class ProductCreatedEvent : DomainEvent
{
    /// <summary>
    /// The ID of the product that was created.
    /// </summary>
    public Guid ProductId { get; }

    /// <summary>
    /// The name of the product.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The barcode of the product, if any.
    /// </summary>
    public string? Barcode { get; }

    /// <summary>
    /// The unit price of the product.
    /// </summary>
    public decimal UnitPrice { get; }

    public ProductCreatedEvent(Guid productId, string name, string? barcode, decimal unitPrice)
    {
        ProductId = productId;
        Name = name;
        Barcode = barcode;
        UnitPrice = unitPrice;
    }
}
