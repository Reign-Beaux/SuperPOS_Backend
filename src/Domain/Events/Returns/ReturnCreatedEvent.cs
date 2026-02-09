namespace Domain.Events.Returns;

/// <summary>
/// Domain event raised when a return is created.
/// </summary>
public sealed class ReturnCreatedEvent : DomainEvent
{
    public Guid ReturnId { get; }
    public Guid SaleId { get; }
    public decimal TotalRefund { get; }

    public ReturnCreatedEvent(Guid returnId, Guid saleId, decimal totalRefund)
    {
        ReturnId = returnId;
        SaleId = saleId;
        TotalRefund = totalRefund;
    }
}
