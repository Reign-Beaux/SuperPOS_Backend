namespace Domain.Entities.Sales;

public class SaleDetail : BaseEntity
{
    public Guid SaleId { get; set; }
    public Guid ArticleId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }

    // Navigation Properties
    public Sale Sale { get; set; } = null!;
    public Articles.Article Article { get; set; } = null!;
}
