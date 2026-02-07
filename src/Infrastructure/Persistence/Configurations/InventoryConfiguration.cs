using Domain.Entities.Inventories;

namespace Infrastructure.Persistence.Configurations;

public sealed class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.ToTable("Inventory");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductId)
          .IsRequired();

        builder.Property(i => i.Stock)
          .IsRequired();

        builder.HasIndex(i => i.ProductId)
          .IsUnique();

        // Relationship
        builder.HasOne(i => i.Product)
          .WithMany(a => a.Inventories)
          .HasForeignKey(i => i.ProductId)
          .OnDelete(DeleteBehavior.Restrict);
    }
}
