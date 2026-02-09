using Domain.Entities.Emails;

namespace Infrastructure.Persistence.Configurations;

public class EmailLogConfiguration : IEntityTypeConfiguration<EmailLog>
{
    public void Configure(EntityTypeBuilder<EmailLog> builder)
    {
        builder.ToTable("EmailLogs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Recipient)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.Subject)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Body)
            .IsRequired();

        builder.Property(e => e.EmailType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(e => e.IsSent)
            .IsRequired();

        // Indexes
        builder.HasIndex(e => e.Recipient);
        builder.HasIndex(e => e.EmailType);
        builder.HasIndex(e => e.IsSent);
        builder.HasIndex(e => e.CreatedAt);
    }
}
