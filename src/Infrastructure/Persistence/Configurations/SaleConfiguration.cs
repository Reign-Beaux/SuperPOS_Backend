using Domain.Entities.Sales;

namespace Infrastructure.Persistence.Configurations;

public sealed class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.CustomerId)
          .IsRequired();

        builder.Property(s => s.UserId)
          .IsRequired();

        builder.Property(s => s.TotalAmount)
          .IsRequired()
          .HasPrecision(18, 2);

        builder.HasIndex(s => s.CustomerId);
        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.CreatedAt);

        // Relationships
        builder.HasOne(s => s.Customer)
          .WithMany(c => c.Sales)
          .HasForeignKey(s => s.CustomerId)
          .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.User)
          .WithMany(u => u.Sales)
          .HasForeignKey(s => s.UserId)
          .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.SaleDetails)
          .WithOne(sd => sd.Sale)
          .HasForeignKey(sd => sd.SaleId)
          .OnDelete(DeleteBehavior.Cascade);
    }
}
