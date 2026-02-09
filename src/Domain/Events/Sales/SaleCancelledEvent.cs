namespace Domain.Events.Sales;

/// <summary>
/// Domain event raised when a sale is cancelled.
/// Triggers automatic inventory restoration.
/// </summary>
public sealed class SaleCancelledEvent : DomainEvent
{
    public Guid SaleId { get; }
    public Guid CancelledByUserId { get; }
    public string Reason { get; }
    public List<SaleItemToRestore> ItemsToRestore { get; }

    public SaleCancelledEvent(
        Guid saleId,
        Guid cancelledByUserId,
        string reason,
        List<SaleItemToRestore> itemsToRestore)
    {
        SaleId = saleId;
        CancelledByUserId = cancelledByUserId;
        Reason = reason;
        ItemsToRestore = itemsToRestore;
    }

    public record SaleItemToRestore(Guid ProductId, int Quantity);
}
