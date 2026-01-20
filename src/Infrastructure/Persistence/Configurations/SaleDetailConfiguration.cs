using Domain.Entities.Sales;

namespace Infrastructure.Persistence.Configurations;

public sealed class SaleDetailConfiguration : IEntityTypeConfiguration<SaleDetail>
{
    public void Configure(EntityTypeBuilder<SaleDetail> builder)
    {
        builder.ToTable("SaleDetails");
        builder.HasKey(sd => sd.Id);

        builder.Property(sd => sd.SaleId)
          .IsRequired();

        builder.Property(sd => sd.ArticleId)
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
        builder.HasIndex(sd => sd.ArticleId);

        // Relationships
        builder.HasOne(sd => sd.Sale)
          .WithMany(s => s.SaleDetails)
          .HasForeignKey(sd => sd.SaleId)
          .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sd => sd.Article)
          .WithMany(a => a.SaleDetails)
          .HasForeignKey(sd => sd.ArticleId)
          .OnDelete(DeleteBehavior.Restrict);
    }
}
