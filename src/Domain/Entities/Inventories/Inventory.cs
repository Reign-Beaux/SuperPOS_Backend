using Domain.Entities.Products;

namespace Domain.Entities.Inventories;

public class Inventory : BaseEntity
{
    public Inventory() { }

    public Guid ProductId { get; set; }
    public int Quantity { get; set; }

    // Navigation Properties
    public Product Product { get; set; } = null!;
}
