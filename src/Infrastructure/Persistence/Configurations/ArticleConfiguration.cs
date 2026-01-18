using Domain.Entities.Articles;

namespace Infrastructure.Persistence.Configurations;

public sealed class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.ToTable("Articles");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
          .IsRequired()
          .HasMaxLength(255);
        builder.Property(a => a.Description)
          .HasMaxLength(500);
        builder.Property(a => a.Barcode)
          .HasMaxLength(100);

        builder.HasIndex(a => a.Name)
          .IsUnique();
        builder.HasIndex(a => a.Barcode)
          .IsUnique()
          .HasFilter("[Barcode] IS NOT NULL");
    }
}