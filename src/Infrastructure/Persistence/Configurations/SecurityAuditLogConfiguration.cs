using Domain.Entities.Security;

namespace Infrastructure.Persistence.Configurations;

public class SecurityAuditLogConfiguration : IEntityTypeConfiguration<SecurityAuditLog>
{
    public void Configure(EntityTypeBuilder<SecurityAuditLog> builder)
    {
        builder.ToTable("SecurityAuditLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .IsRequired(false);

        builder.Property(x => x.EventType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.IpAddress)
            .HasMaxLength(45); // IPv6 max length

        builder.Property(x => x.UserAgent)
            .HasMaxLength(500);

        builder.Property(x => x.Details)
            .HasMaxLength(2000);

        builder.Property(x => x.IsSuccess)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Indexes for common queries
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.EventType);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => new { x.EventType, x.IsSuccess });
    }
}
