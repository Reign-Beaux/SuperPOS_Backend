using Domain.Entities.Customers;
using Domain.Entities.Sales;
using Domain.Entities.Users;
using Domain.Events.Returns;
using Domain.Exceptions;

namespace Domain.Entities.Returns;

/// <summary>
/// Return aggregate root representing a product return or exchange.
/// </summary>
public class Return : BaseEntity, IAggregateRoot
{
    private readonly List<ReturnDetail> _returnDetails = [];

    // Parameterless constructor for EF Core
    public Return() { }

    // Properties
    public Guid SaleId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid ProcessedByUserId { get; private set; }
    public decimal TotalRefund { get; private set; }
    public ReturnType Type { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public ReturnStatus Status { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public Guid? RejectedByUserId { get; private set; }
    public DateTime? RejectedAt { get; private set; }
    public string? RejectionReason { get; private set; }

    // Navigation Properties
    public Sale Sale { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public User ProcessedByUser { get; set; } = null!;

    /// <summary>
    /// Read-only access to return details.
    /// </summary>
    public IReadOnlyCollection<ReturnDetail> ReturnDetailsReadOnly => _returnDetails.AsReadOnly();

    /// <summary>
    /// EF Core needs a setter for the collection.
    /// </summary>
    public ICollection<ReturnDetail> ReturnDetails
    {
        get => _returnDetails;
        set
        {
            _returnDetails.Clear();
            if (value != null)
            {
                foreach (var item in value)
                {
                    _returnDetails.Add(item);
                }
            }
        }
    }

    /// <summary>
    /// Factory method to create a new return.
    /// </summary>
    public static Return Create(
        Guid saleId,
        Guid customerId,
        Guid processedByUserId,
        ReturnType type,
        string reason,
        List<(Guid ProductId, int Quantity, decimal UnitPrice, string? Condition)> items)
    {
        if (saleId == Guid.Empty)
            throw new BusinessRuleViolationException("RETURN_001", "Sale ID cannot be empty");

        if (customerId == Guid.Empty)
            throw new BusinessRuleViolationException("RETURN_002", "Customer ID cannot be empty");

        if (processedByUserId == Guid.Empty)
            throw new BusinessRuleViolationException("RETURN_003", "Processed by user ID cannot be empty");

        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessRuleViolationException("RETURN_004", "Reason is required");

        if (items == null || items.Count == 0)
            throw new BusinessRuleViolationException("RETURN_005", "Return must have at least one item");

        var returnEntity = new Return
        {
            SaleId = saleId,
            CustomerId = customerId,
            ProcessedByUserId = processedByUserId,
            Type = type,
            Reason = reason,
            Status = ReturnStatus.Pending
        };

        // Add items
        foreach (var (productId, quantity, unitPrice, condition) in items)
        {
            returnEntity.AddItem(productId, quantity, unitPrice, condition);
        }

        // Calculate total refund
        returnEntity.RecalculateTotal();

        // Raise domain event
        returnEntity.AddDomainEvent(new ReturnCreatedEvent(returnEntity.Id, saleId, returnEntity.TotalRefund));

        return returnEntity;
    }

    private void AddItem(Guid productId, int quantity, decimal unitPrice, string? condition)
    {
        if (productId == Guid.Empty)
            throw new BusinessRuleViolationException("RETURN_006", "Product ID cannot be empty");

        if (quantity <= 0)
            throw new BusinessRuleViolationException("RETURN_007", "Quantity must be greater than zero");

        if (unitPrice < 0)
            throw new BusinessRuleViolationException("RETURN_008", "Unit price cannot be negative");

        // Check for duplicate product
        if (_returnDetails.Any(d => d.ProductId == productId))
            throw new BusinessRuleViolationException("RETURN_009", $"Product {productId} is already in this return");

        var detail = ReturnDetail.Create(productId, quantity, unitPrice, condition);
        _returnDetails.Add(detail);
    }

    private void RecalculateTotal()
    {
        TotalRefund = _returnDetails.Sum(d => d.Total);
    }

    /// <summary>
    /// Approves the return and raises domain event for inventory restoration.
    /// </summary>
    public void Approve(Guid approvedByUserId)
    {
        if (Status != ReturnStatus.Pending)
            throw new BusinessRuleViolationException("RETURN_010", "Only pending returns can be approved");

        if (approvedByUserId == Guid.Empty)
            throw new BusinessRuleViolationException("RETURN_011", "Approved by user ID cannot be empty");

        Status = ReturnStatus.Approved;
        ApprovedByUserId = approvedByUserId;
        ApprovedAt = DateTime.UtcNow;

        // Prepare items to restock
        var itemsToRestock = _returnDetails
            .Select(d => new ReturnApprovedEvent.ItemToRestock(d.ProductId, d.Quantity))
            .ToList();

        // Raise domain event for inventory restoration
        AddDomainEvent(new ReturnApprovedEvent(Id, SaleId, itemsToRestock));
    }

    /// <summary>
    /// Rejects the return.
    /// </summary>
    public void Reject(Guid rejectedByUserId, string rejectionReason)
    {
        if (Status != ReturnStatus.Pending)
            throw new BusinessRuleViolationException("RETURN_012", "Only pending returns can be rejected");

        if (rejectedByUserId == Guid.Empty)
            throw new BusinessRuleViolationException("RETURN_013", "Rejected by user ID cannot be empty");

        if (string.IsNullOrWhiteSpace(rejectionReason))
            throw new BusinessRuleViolationException("RETURN_014", "Rejection reason is required");

        Status = ReturnStatus.Rejected;
        RejectedByUserId = rejectedByUserId;
        RejectedAt = DateTime.UtcNow;
        RejectionReason = rejectionReason;

        // No domain event needed for rejection
    }

    /// <summary>
    /// Finalizes the return details by setting ReturnId on all details.
    /// </summary>
    public void FinalizeDetails()
    {
        if (Id == Guid.Empty)
            throw new BusinessRuleViolationException("RETURN_015", "Cannot finalize details before Return is saved");

        foreach (var detail in _returnDetails)
        {
            if (detail.ReturnId == Guid.Empty)
            {
                detail.SetReturnId(Id);
            }
        }
    }
}
