using Domain.Entities.Roles;

namespace Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
          .IsRequired()
          .HasMaxLength(100);

        builder.Property(r => r.Description)
          .HasMaxLength(500);

        builder.HasIndex(r => r.Name)
          .IsUnique()
          .HasFilter("[DeletedAt] IS NULL");

        // Note: Role-User relationship is configured in UserConfiguration
    }
}
