using Domain.Entities.Products;

namespace Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
          .IsRequired()
          .HasMaxLength(255);
        builder.Property(a => a.Description)
          .HasMaxLength(500);
        builder.Property(a => a.Barcode)
          .HasMaxLength(100);
        builder.Property(a => a.UnitPrice)
          .IsRequired()
          .HasPrecision(18, 2);

        builder.HasIndex(a => a.Name)
          .IsUnique();
        builder.HasIndex(a => a.Barcode)
          .IsUnique()
          .HasFilter("[Barcode] IS NOT NULL");

        // Relationships
        builder.HasMany(a => a.Inventories)
          .WithOne(i => i.Product)
          .HasForeignKey(i => i.ProductId)
          .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.SaleDetails)
          .WithOne(sd => sd.Product)
          .HasForeignKey(sd => sd.ProductId)
          .OnDelete(DeleteBehavior.Restrict);
    }
}