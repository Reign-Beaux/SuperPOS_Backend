using Domain.Entities.Customers;
using Domain.Entities.Users;

namespace Domain.Entities.Sales;

public class Sale : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Guid UserId { get; set; }
    public decimal TotalAmount { get; set; }

    // Navigation Properties
    public Customer Customer { get; set; } = null!;
    public User User { get; set; } = null!;
    public ICollection<SaleDetail> SaleDetails { get; set; } = [];
}
