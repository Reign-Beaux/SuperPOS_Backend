using Domain.Entities.CashRegisters;
using Domain.Entities.Users;

namespace Infrastructure.Persistence.Configurations;

public class CashRegisterConfiguration : IEntityTypeConfiguration<CashRegister>
{
    public void Configure(EntityTypeBuilder<CashRegister> builder)
    {
        builder.ToTable("CashRegisters");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.InitialCash)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(c => c.FinalCash)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(c => c.TotalSales)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(c => c.Difference)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(c => c.Notes)
            .HasMaxLength(500);

        builder.Property(c => c.OpeningDate)
            .IsRequired();

        builder.Property(c => c.ClosingDate)
            .IsRequired();

        // Relación con User
        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices para mejorar performance de consultas
        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.ClosingDate);
        builder.HasIndex(c => new { c.UserId, c.DeletedAt });

        // Query filter para soft delete
        builder.HasQueryFilter(c => c.DeletedAt == null);
    }
}
