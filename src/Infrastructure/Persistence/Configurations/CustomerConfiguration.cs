using Domain.Entities.Customers;

namespace Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
          .IsRequired()
          .HasMaxLength(100);

        builder.Property(c => c.FirstLastname)
          .IsRequired()
          .HasMaxLength(100);

        builder.Property(c => c.SecondLastname)
          .HasMaxLength(100);

        builder.Property(c => c.Phone)
          .HasMaxLength(20);

        builder.Property(c => c.Email)
          .HasMaxLength(255);

        builder.Property(c => c.BirthDate);

        builder.HasIndex(c => c.Email)
          .IsUnique()
          .HasFilter("[Email] IS NOT NULL AND [DeletedAt] IS NULL");

        // Relationships
        builder.HasMany(c => c.Sales)
          .WithOne(s => s.Customer)
          .HasForeignKey(s => s.CustomerId)
          .OnDelete(DeleteBehavior.Restrict);
    }
}
