namespace Domain.Events.Products;

/// <summary>
/// Domain event raised when a product's price is changed.
/// This event can trigger side effects like:
/// - Notifying customers of price changes
/// - Updating pricing in external systems
/// - Recording price history for audits
/// - Updating promotional materials
/// </summary>
public sealed class ProductPriceChangedEvent : DomainEvent
{
    /// <summary>
    /// The ID of the product whose price changed.
    /// </summary>
    public Guid ProductId { get; }

    /// <summary>
    /// The previous price before the change.
    /// </summary>
    public decimal OldPrice { get; }

    /// <summary>
    /// The new price after the change.
    /// </summary>
    public decimal NewPrice { get; }

    /// <summary>
    /// The percentage change in price.
    /// Positive for price increase, negative for decrease.
    /// </summary>
    public decimal PercentageChange { get; }

    public ProductPriceChangedEvent(Guid productId, decimal oldPrice, decimal newPrice)
    {
        ProductId = productId;
        OldPrice = oldPrice;
        NewPrice = newPrice;
        PercentageChange = oldPrice != 0 ? ((newPrice - oldPrice) / oldPrice) * 100 : 0;
    }
}
