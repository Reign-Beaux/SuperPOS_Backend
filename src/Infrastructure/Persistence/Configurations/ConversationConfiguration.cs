using Domain.Entities.Chat;

namespace Infrastructure.Persistence.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.User1Id)
            .IsRequired();

        builder.Property(c => c.User2Id)
            .IsRequired();

        builder.Property(c => c.LastMessageAt)
            .IsRequired();

        // Relationship with ChatMessages
        builder.HasMany(c => c.Messages)
            .WithOne(cm => cm.Conversation)
            .HasForeignKey(cm => cm.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(c => c.User1Id);
        builder.HasIndex(c => c.User2Id);
        builder.HasIndex(c => c.LastMessageAt);

        // Composite index for finding conversation between two users
        builder.HasIndex(c => new { c.User1Id, c.User2Id })
            .IsUnique();

        // Ignore read-only collection
        builder.Ignore(c => c.MessagesReadOnly);
    }
}
