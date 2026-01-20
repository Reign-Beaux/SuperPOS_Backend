namespace Domain.Entities.Sales;

public class Sale : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Guid UserId { get; set; }
    public decimal TotalAmount { get; set; }

    // Navigation Properties
    public Customers.Customer Customer { get; set; } = null!;
    public Users.User User { get; set; } = null!;
    public ICollection<SaleDetail> SaleDetails { get; set; } = [];
}
