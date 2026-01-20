using Domain.Entities.Users;

namespace Infrastructure.Persistence.Configurations;

public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");
        
        // Composite Primary Key
        builder.HasKey(ur => new { ur.UserId, ur.RoleId });

        builder.Property(ur => ur.UserId)
          .IsRequired();

        builder.Property(ur => ur.RoleId)
          .IsRequired();

        // Relationships
        builder.HasOne(ur => ur.User)
          .WithMany(u => u.UserRoles)
          .HasForeignKey(ur => ur.UserId)
          .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ur => ur.Role)
          .WithMany(r => r.UserRoles)
          .HasForeignKey(ur => ur.RoleId)
          .OnDelete(DeleteBehavior.Cascade);
    }
}
