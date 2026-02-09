namespace Domain.Events.Returns;

/// <summary>
/// Domain event raised when a return is approved.
/// Triggers inventory restoration.
/// </summary>
public sealed class ReturnApprovedEvent : DomainEvent
{
    public Guid ReturnId { get; }
    public Guid SaleId { get; }
    public List<ItemToRestock> ItemsToRestock { get; }

    public ReturnApprovedEvent(Guid returnId, Guid saleId, List<ItemToRestock> itemsToRestock)
    {
        ReturnId = returnId;
        SaleId = saleId;
        ItemsToRestock = itemsToRestock;
    }

    public record ItemToRestock(Guid ProductId, int Quantity);
}
