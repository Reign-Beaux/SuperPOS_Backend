namespace Domain.Entities.Articles;

public class Article : BaseCatalog
{
    public string? Barcode { get; set; }
    public decimal UnitPrice { get; set; }

    // Navigation Properties
    public ICollection<Inventories.Inventory> Inventories { get; set; } = [];
    public ICollection<Sales.SaleDetail> SaleDetails { get; set; } = [];
}
