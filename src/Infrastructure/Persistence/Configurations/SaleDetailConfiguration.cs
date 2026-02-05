using Domain.Entities.Sales;

namespace Infrastructure.Persistence.Configurations;

public sealed class SaleDetailConfiguration : IEntityTypeConfiguration<SaleDetail>
{
    public void Configure(EntityTypeBuilder<SaleDetail> builder)
    {
        builder.ToTable("SaleDetails");
        builder.HasKey(sd => sd.Id);

        // Explicitly configure SaleId as a mapped property
        builder.Property(sd => sd.SaleId)
          .HasColumnName("SaleId")
          .IsRequired();

        builder.Property(sd => sd.ProductId)
          .IsRequired();

        builder.Property(sd => sd.Quantity)
          .IsRequired();

        builder.Property(sd => sd.UnitPrice)
          .IsRequired()
          .HasPrecision(18, 2);

        builder.Property(sd => sd.Total)
          .IsRequired()
          .HasPrecision(18, 2);

        builder.HasIndex(sd => sd.SaleId);
        builder.HasIndex(sd => sd.ProductId);

        // Relationships
        builder.HasOne(sd => sd.Sale)
          .WithMany(s => s.SaleDetails)
          .HasForeignKey(sd => sd.SaleId)
          .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sd => sd.Product)
          .WithMany(a => a.SaleDetails)
          .HasForeignKey(sd => sd.ProductId)
          .OnDelete(DeleteBehavior.Restrict);
    }
}
