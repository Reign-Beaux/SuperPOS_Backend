using Domain.Entities.Inventories;

namespace Infrastructure.Persistence.Configurations;

public sealed class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.ToTable("Inventory");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ArticleId)
          .IsRequired();

        builder.Property(i => i.Quantity)
          .IsRequired();

        builder.HasIndex(i => i.ArticleId)
          .IsUnique();

        // Relationship
        builder.HasOne(i => i.Article)
          .WithMany(a => a.Inventories)
          .HasForeignKey(i => i.ArticleId)
          .OnDelete(DeleteBehavior.Restrict);
    }
}
