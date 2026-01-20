namespace Domain.Entities.Inventories;

public class Inventory : BaseEntity
{
    public Guid ArticleId { get; set; }
    public int Quantity { get; set; }

    // Navigation Properties
    public Articles.Article Article { get; set; } = null!;
}
