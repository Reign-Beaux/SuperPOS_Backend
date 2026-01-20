using Domain.Entities.Sales;

namespace Domain.Entities.Articles;

public class Article : BaseCatalog
{
    public Article() { }

    public string? Barcode { get; set; }
    public decimal UnitPrice { get; set; }

    // Navigation Properties
    public ICollection<Inventories.Inventory> Inventories { get; set; } = [];
    public ICollection<SaleDetail> SaleDetails { get; set; } = [];
}
