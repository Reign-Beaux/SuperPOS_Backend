namespace Domain.Events.Inventories;

/// <summary>
/// Domain event raised when inventory stock falls below a threshold.
/// This event can trigger side effects like:
/// - Sending low stock notifications to managers
/// - Automatically creating purchase orders
/// - Alerting warehouse staff
/// - Updating inventory dashboards
/// </summary>
public sealed class LowStockEvent : DomainEvent
{
    /// <summary>
    /// The ID of the inventory record with low stock.
    /// </summary>
    public Guid InventoryId { get; }

    /// <summary>
    /// The ID of the product that has low stock.
    /// </summary>
    public Guid ProductId { get; }

    /// <summary>
    /// The current stock quantity.
    /// </summary>
    public int CurrentQuantity { get; }

    /// <summary>
    /// The threshold that triggered this alert.
    /// </summary>
    public int Threshold { get; }

    public LowStockEvent(
        Guid inventoryId,
        Guid productId,
        int currentQuantity,
        int threshold)
    {
        InventoryId = inventoryId;
        ProductId = productId;
        CurrentQuantity = currentQuantity;
        Threshold = threshold;
    }
}
