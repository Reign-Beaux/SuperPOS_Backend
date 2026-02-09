using Domain.Entities.Sales;

namespace Infrastructure.Persistence.Configurations;

public sealed class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");
        builder.HasKey(s => s.Id);

        // Ignore read-only property - use SaleDetails for EF Core navigation
        builder.Ignore(s => s.SaleDetailsReadOnly);

        builder.Property(s => s.CustomerId)
          .IsRequired();

        builder.Property(s => s.UserId)
          .IsRequired();

        builder.Property(s => s.TotalAmount)
          .IsRequired()
          .HasPrecision(18, 2);

        // Cancellation properties
        builder.Property(s => s.IsCancelled)
          .IsRequired()
          .HasDefaultValue(false);

        builder.Property(s => s.CancelledAt);

        builder.Property(s => s.CancelledByUserId);

        builder.Property(s => s.CancellationReason)
          .HasMaxLength(500);

        builder.HasIndex(s => s.CustomerId);
        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.CreatedAt);
        builder.HasIndex(s => s.IsCancelled);

        // Relationships
        builder.HasOne(s => s.Customer)
          .WithMany(c => c.Sales)
          .HasForeignKey(s => s.CustomerId)
          .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.User)
          .WithMany(u => u.Sales)
          .HasForeignKey(s => s.UserId)
          .OnDelete(DeleteBehavior.Restrict);

        // Note: Sale-SaleDetail relationship is configured in SaleDetailConfiguration
    }
}
