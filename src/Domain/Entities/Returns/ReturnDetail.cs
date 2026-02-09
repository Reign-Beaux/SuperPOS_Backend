using Domain.Entities.Products;
using Domain.Exceptions;

namespace Domain.Entities.Returns;

/// <summary>
/// Return detail representing a single item in a return transaction.
/// </summary>
public class ReturnDetail : BaseEntity
{
    // Parameterless constructor for EF Core
    public ReturnDetail() { }

    public Guid ReturnId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Total { get; private set; }
    public string? Condition { get; private set; }

    // Navigation Properties
    public Return Return { get; set; } = null!;
    public Product Product { get; set; } = null!;

    /// <summary>
    /// Internal factory method (called only by Return aggregate).
    /// </summary>
    internal static ReturnDetail Create(Guid productId, int quantity, decimal unitPrice, string? condition)
    {
        if (productId == Guid.Empty)
            throw new BusinessRuleViolationException("RETURN_DETAIL_001", "Product ID cannot be empty");

        if (quantity <= 0)
            throw new BusinessRuleViolationException("RETURN_DETAIL_002", "Quantity must be greater than zero");

        if (unitPrice < 0)
            throw new BusinessRuleViolationException("RETURN_DETAIL_003", "Unit price cannot be negative");

        var detail = new ReturnDetail
        {
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Condition = condition
        };

        detail.CalculateTotal();
        return detail;
    }

    private void CalculateTotal()
    {
        Total = UnitPrice * Quantity;
    }

    /// <summary>
    /// Sets the ReturnId (called by Return aggregate after saving).
    /// </summary>
    internal void SetReturnId(Guid returnId)
    {
        if (ReturnId != Guid.Empty)
            throw new BusinessRuleViolationException("RETURN_DETAIL_004", "ReturnId already set");

        if (returnId == Guid.Empty)
            throw new BusinessRuleViolationException("RETURN_DETAIL_005", "ReturnId cannot be empty");

        ReturnId = returnId;
    }
}
