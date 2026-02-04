namespace Domain.Events.Inventories;

/// <summary>
/// Domain event raised when stock is removed from inventory.
/// This event can trigger side effects like:
/// - Checking for low stock alerts
/// - Updating warehouse management systems
/// - Triggering automatic reordering
/// - Recording inventory audit trails
/// </summary>
public sealed class StockDecrementedEvent : DomainEvent
{
    /// <summary>
    /// The ID of the inventory record that was decremented.
    /// </summary>
    public Guid InventoryId { get; }

    /// <summary>
    /// The ID of the product whose stock was decremented.
    /// </summary>
    public Guid ProductId { get; }

    /// <summary>
    /// The quantity that was removed.
    /// </summary>
    public int QuantityRemoved { get; }

    /// <summary>
    /// The remaining quantity after the decrement.
    /// </summary>
    public int RemainingQuantity { get; }

    public StockDecrementedEvent(
        Guid inventoryId,
        Guid productId,
        int quantityRemoved,
        int remainingQuantity)
    {
        InventoryId = inventoryId;
        ProductId = productId;
        QuantityRemoved = quantityRemoved;
        RemainingQuantity = remainingQuantity;
    }
}
