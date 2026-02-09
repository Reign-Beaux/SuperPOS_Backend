using Domain.Entities.Chat;

namespace Infrastructure.Persistence.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");

        builder.HasKey(cm => cm.Id);

        builder.Property(cm => cm.ConversationId)
            .IsRequired();

        builder.Property(cm => cm.SenderId)
            .IsRequired();

        builder.Property(cm => cm.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(cm => cm.SentAt)
            .IsRequired();

        builder.Property(cm => cm.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationship with Conversation
        builder.HasOne(cm => cm.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(cm => cm.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(cm => cm.ConversationId);
        builder.HasIndex(cm => cm.SenderId);
        builder.HasIndex(cm => cm.SentAt);
        builder.HasIndex(cm => cm.IsRead);
    }
}
