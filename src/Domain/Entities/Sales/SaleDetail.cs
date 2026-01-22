using Domain.Entities.Products;

namespace Domain.Entities.Sales;

public class SaleDetail : BaseEntity
{
    public SaleDetail() { }

    public Guid SaleId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }

    // Navigation Properties
    public Sale Sale { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
