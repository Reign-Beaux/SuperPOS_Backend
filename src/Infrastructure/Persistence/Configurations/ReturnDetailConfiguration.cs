using Domain.Entities.Returns;

namespace Infrastructure.Persistence.Configurations;

public class ReturnDetailConfiguration : IEntityTypeConfiguration<ReturnDetail>
{
    public void Configure(EntityTypeBuilder<ReturnDetail> builder)
    {
        builder.ToTable("ReturnDetails");
        builder.HasKey(rd => rd.Id);

        builder.Property(rd => rd.ReturnId).IsRequired();
        builder.Property(rd => rd.ProductId).IsRequired();
        builder.Property(rd => rd.Quantity).IsRequired();
        builder.Property(rd => rd.UnitPrice).IsRequired().HasPrecision(18, 2);
        builder.Property(rd => rd.Total).IsRequired().HasPrecision(18, 2);
        builder.Property(rd => rd.Condition).HasMaxLength(200);

        builder.HasIndex(rd => rd.ReturnId);
        builder.HasIndex(rd => rd.ProductId);

        // Relationships
        builder.HasOne(rd => rd.Return)
            .WithMany(r => r.ReturnDetails)
            .HasForeignKey(rd => rd.ReturnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rd => rd.Product)
            .WithMany()
            .HasForeignKey(rd => rd.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
