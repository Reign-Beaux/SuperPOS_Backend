using Domain.Entities.Authentication;

namespace Infrastructure.Persistence.Configurations;

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("PasswordResetTokens");

        builder.HasKey(prt => prt.Id);

        builder.Property(prt => prt.UserId)
            .IsRequired();

        builder.Property(prt => prt.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(prt => prt.ExpiresAt)
            .IsRequired();

        builder.Property(prt => prt.AttemptCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(prt => prt.IsUsed)
            .IsRequired()
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(prt => prt.UserId);
        builder.HasIndex(prt => prt.Code);
        builder.HasIndex(prt => prt.ExpiresAt);

        // Ignore computed properties
        builder.Ignore(prt => prt.IsExpired);
        builder.Ignore(prt => prt.IsValid);
    }
}
