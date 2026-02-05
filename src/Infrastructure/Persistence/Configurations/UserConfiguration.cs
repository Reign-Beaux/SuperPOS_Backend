using Domain.Entities.Users;

namespace Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
          .IsRequired()
          .HasMaxLength(100);

        builder.Property(u => u.FirstLastname)
          .IsRequired()
          .HasMaxLength(100);

        builder.Property(u => u.SecondLastname)
          .HasMaxLength(100);

        builder.Property(u => u.Email)
          .IsRequired()
          .HasMaxLength(255);

        builder.Property(u => u.PasswordHashed)
          .IsRequired()
          .HasMaxLength(500);

        builder.Property(u => u.Phone)
          .HasMaxLength(20);

        builder.Property(u => u.RoleId)
          .IsRequired();

        builder.HasIndex(u => u.Email)
          .IsUnique();

        builder.HasIndex(u => u.RoleId);

        // Relationships
        builder.HasOne(u => u.Role)
          .WithMany(r => r.Users)
          .HasForeignKey(u => u.RoleId)
          .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Sales)
          .WithOne(s => s.User)
          .HasForeignKey(s => s.UserId)
          .OnDelete(DeleteBehavior.Restrict);
    }
}
