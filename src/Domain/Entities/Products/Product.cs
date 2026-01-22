using Domain.Entities.Sales;

namespace Domain.Entities.Products;

public class Product : BaseCatalog
{
    public Product() { }

    public string? Barcode { get; set; }
    public decimal UnitPrice { get; set; }

    // Navigation Properties
    public ICollection<Inventories.Inventory> Inventories { get; set; } = [];
    public ICollection<SaleDetail> SaleDetails { get; set; } = [];
}
