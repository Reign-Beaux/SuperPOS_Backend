namespace Domain.Entities.Returns;

/// <summary>
/// Type of return transaction.
/// </summary>
public enum ReturnType
{
    /// <summary>
    /// Refund - Customer gets money back
    /// </summary>
    Refund = 1,

    /// <summary>
    /// Exchange - Customer exchanges for another product
    /// </summary>
    Exchange = 2
}
