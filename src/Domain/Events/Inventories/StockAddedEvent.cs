namespace Domain.Events.Inventories;

/// <summary>
/// Domain event raised when stock is added to inventory.
/// This event can trigger side effects like:
/// - Updating warehouse management systems
/// - Recording inventory receipts
/// - Notifying sales team of available stock
/// - Updating inventory dashboards
/// </summary>
public sealed class StockAddedEvent : DomainEvent
{
    /// <summary>
    /// The ID of the inventory record that was incremented.
    /// </summary>
    public Guid InventoryId { get; }

    /// <summary>
    /// The ID of the product whose stock was added.
    /// </summary>
    public Guid ProductId { get; }

    /// <summary>
    /// The quantity that was added.
    /// </summary>
    public int QuantityAdded { get; }

    /// <summary>
    /// The new total quantity after the addition.
    /// </summary>
    public int NewTotalQuantity { get; }

    public StockAddedEvent(
        Guid inventoryId,
        Guid productId,
        int quantityAdded,
        int newTotalQuantity)
    {
        InventoryId = inventoryId;
        ProductId = productId;
        QuantityAdded = quantityAdded;
        NewTotalQuantity = newTotalQuantity;
    }
}
