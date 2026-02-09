using Domain.Entities.Returns;

namespace Infrastructure.Persistence.Configurations;

public class ReturnConfiguration : IEntityTypeConfiguration<Return>
{
    public void Configure(EntityTypeBuilder<Return> builder)
    {
        builder.ToTable("Returns");
        builder.HasKey(r => r.Id);

        builder.Ignore(r => r.ReturnDetailsReadOnly);

        builder.Property(r => r.SaleId).IsRequired();
        builder.Property(r => r.CustomerId).IsRequired();
        builder.Property(r => r.ProcessedByUserId).IsRequired();
        builder.Property(r => r.TotalRefund).IsRequired().HasPrecision(18, 2);
        builder.Property(r => r.Type).IsRequired();
        builder.Property(r => r.Reason).IsRequired().HasMaxLength(500);
        builder.Property(r => r.Status).IsRequired();
        builder.Property(r => r.RejectionReason).HasMaxLength(500);

        builder.HasIndex(r => r.SaleId);
        builder.HasIndex(r => r.CustomerId);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.CreatedAt);

        // Relationships
        builder.HasOne(r => r.Sale)
            .WithMany()
            .HasForeignKey(r => r.SaleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ProcessedByUser)
            .WithMany()
            .HasForeignKey(r => r.ProcessedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
