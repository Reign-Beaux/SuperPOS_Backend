using Domain.Entities.Articles;

namespace Domain.Entities.Inventories;

public class Inventory : BaseEntity
{
    public Guid ArticleId { get; set; }
    public int Quantity { get; set; }

    // Navigation Properties
    public Article Article { get; set; } = null!;
}
