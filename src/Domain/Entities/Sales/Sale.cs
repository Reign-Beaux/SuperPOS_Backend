using Domain.Entities.Customers;
using Domain.Entities.Users;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities.Sales;

/// <summary>
/// Sale aggregate root representing a sales transaction.
/// Protects invariants: TotalAmount must equal sum of SaleDetails.
/// </summary>
public class Sale : BaseEntity, IAggregateRoot
{
    // Backing field for value object
    private Money? _totalAmount;

    // Backing field for protected collection
    private readonly List<SaleDetail> _saleDetails = [];

    // Parameterless constructor required by EF Core
    public Sale() { }

    // Primitive properties for EF Core persistence
    public Guid CustomerId { get; set; }
    public Guid UserId { get; set; }
    public decimal TotalAmount { get; private set; }  // Private setter to protect invariant

    // Value Object property for domain logic
    public Money TotalAmountValue
    {
        get => _totalAmount ??= Money.Create(TotalAmount);
        private set => _totalAmount = value;
    }

    // Navigation Properties
    public Customer Customer { get; set; } = null!;
    public User User { get; set; } = null!;

    /// <summary>
    /// Read-only access to sale details for external code.
    /// Use this for queries and display.
    /// </summary>
    public IReadOnlyCollection<SaleDetail> SaleDetailsReadOnly => _saleDetails.AsReadOnly();

    /// <summary>
    /// EF Core needs a setter for the collection.
    /// This allows EF to populate the collection but external code uses SaleDetailsReadOnly.
    /// </summary>
    public ICollection<SaleDetail> SaleDetails
    {
        get => _saleDetails;
        set
        {
            _saleDetails.Clear();
            if (value != null)
            {
                foreach (var item in value)
                {
                    _saleDetails.Add(item);
                }
            }
        }
    }

    /// <summary>
    /// Factory method to create a new Sale with items.
    /// Validates business rules and calculates total.
    /// </summary>
    /// <param name="customerId">Customer making the purchase</param>
    /// <param name="userId">User processing the sale</param>
    /// <param name="items">List of items with (ProductId, Quantity, UnitPrice)</param>
    public static Sale Create(
        Guid customerId,
        Guid userId,
        List<(Guid ProductId, Quantity Quantity, Money UnitPrice)> items)
    {
        if (customerId == Guid.Empty)
            throw new BusinessRuleViolationException("SALE_001", "Customer ID cannot be empty");

        if (userId == Guid.Empty)
            throw new BusinessRuleViolationException("SALE_002", "User ID cannot be empty");

        if (items == null || items.Count == 0)
            throw new BusinessRuleViolationException("SALE_003", "Sale must have at least one item");

        var sale = new Sale
        {
            CustomerId = customerId,
            UserId = userId
        };

        // Add each item to the sale
        foreach (var (productId, quantity, unitPrice) in items)
        {
            sale.AddItem(productId, quantity, unitPrice);
        }

        // Calculate and set total
        sale.RecalculateTotal();

        return sale;
    }

    /// <summary>
    /// Adds an item to the sale.
    /// Ensures no duplicate products in the same sale.
    /// </summary>
    private void AddItem(Guid productId, Quantity quantity, Money unitPrice)
    {
        // Check for duplicate product
        if (_saleDetails.Any(d => d.ProductId == productId))
            throw new BusinessRuleViolationException("SALE_004", $"Product {productId} is already in this sale");

        // Create sale detail using internal factory
        var detail = SaleDetail.Create(productId, quantity, unitPrice);
        _saleDetails.Add(detail);
    }

    /// <summary>
    /// Recalculates the total amount based on all sale details.
    /// Ensures invariant: TotalAmount = sum of all detail totals.
    /// </summary>
    private void RecalculateTotal()
    {
        var total = _saleDetails.Sum(d => d.Total);
        TotalAmount = total;
        _totalAmount = Money.Create(total);
    }

    /// <summary>
    /// Finalizes the sale by setting SaleId on all details.
    /// Call this after the Sale has been saved and has an Id.
    /// </summary>
    public void FinalizeDetails()
    {
        if (Id == Guid.Empty)
            throw new BusinessRuleViolationException("SALE_005", "Cannot finalize details before Sale is saved");

        foreach (var detail in _saleDetails)
        {
            if (detail.SaleId == Guid.Empty)
            {
                detail.SetSaleId(Id);
            }
        }
    }

    /// <summary>
    /// Gets the count of items in this sale.
    /// </summary>
    public int GetItemCount() => _saleDetails.Count;

    /// <summary>
    /// Gets the total quantity of all items in this sale.
    /// </summary>
    public int GetTotalQuantity() => _saleDetails.Sum(d => d.Quantity);

    /// <summary>
    /// Gets a specific sale detail by product ID.
    /// </summary>
    public SaleDetail? GetDetailByProductId(Guid productId)
    {
        return _saleDetails.FirstOrDefault(d => d.ProductId == productId);
    }

    /// <summary>
    /// Checks if the sale contains a specific product.
    /// </summary>
    public bool ContainsProduct(Guid productId)
    {
        return _saleDetails.Any(d => d.ProductId == productId);
    }

    /// <summary>
    /// Gets the sale total as a formatted string.
    /// </summary>
    public string GetFormattedTotal() => TotalAmountValue.ToString();

    /// <summary>
    /// Validates that all sale details have valid totals.
    /// Used to ensure data integrity.
    /// </summary>
    public bool ValidateTotals()
    {
        return _saleDetails.All(d =>
        {
            var expectedTotal = d.UnitPrice * d.Quantity;
            return Math.Abs(d.Total - expectedTotal) < 0.01m; // Allow small rounding differences
        });
    }
}
