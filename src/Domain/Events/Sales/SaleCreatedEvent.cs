namespace Domain.Events.Sales;

/// <summary>
/// Domain event raised when a new sale is successfully created.
/// This event can trigger side effects like:
/// - Sending receipt emails
/// - Updating analytics
/// - Notifying inventory management
/// - Recording audit logs
/// </summary>
public sealed class SaleCreatedEvent : DomainEvent
{
    /// <summary>
    /// The ID of the sale that was created.
    /// </summary>
    public Guid SaleId { get; }

    /// <summary>
    /// The ID of the customer who made the purchase.
    /// </summary>
    public Guid CustomerId { get; }

    /// <summary>
    /// The ID of the user who processed the sale.
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// The total amount of the sale.
    /// </summary>
    public decimal TotalAmount { get; }

    /// <summary>
    /// The products and quantities included in the sale.
    /// </summary>
    public IReadOnlyList<SaleItemInfo> Items { get; }

    public SaleCreatedEvent(
        Guid saleId,
        Guid customerId,
        Guid userId,
        decimal totalAmount,
        IReadOnlyList<SaleItemInfo> items)
    {
        SaleId = saleId;
        CustomerId = customerId;
        UserId = userId;
        TotalAmount = totalAmount;
        Items = items;
    }

    /// <summary>
    /// Information about an item in the sale.
    /// </summary>
    public record SaleItemInfo(Guid ProductId, int Quantity, decimal UnitPrice);
}
