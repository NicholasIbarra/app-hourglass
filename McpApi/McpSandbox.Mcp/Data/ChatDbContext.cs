using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using McpSandbox.Mcp.Domain.Entities;

namespace McpSandbox.Mcp.Data;

public sealed class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options)
        : base(options)
    {
    }

    public DbSet<Conversation> Conversations => Set<Conversation>();

    public DbSet<ConversationMessage> ConversationMessages => Set<ConversationMessage>();

    public DbSet<Prompt> Prompts => Set<Prompt>();

    public DbSet<PromptVersion> PromptVersions => Set<PromptVersion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        static void ConfigureBaseEntity<TEntity>(EntityTypeBuilder<TEntity> entity)
            where TEntity : BaseEntity
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.UpdatedAt).IsRequired(false);
        }

        modelBuilder.Entity<Conversation>(entity =>
        {
            ConfigureBaseEntity(entity);

            entity.Property(e => e.Title).HasMaxLength(500);

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(32);

            entity.HasIndex(e => e.UserId);

            entity.HasMany(e => e.Messages)
                .WithOne(e => e.Conversation)
                .HasForeignKey(e => e.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ConversationMessage>(entity =>
        {
            ConfigureBaseEntity(entity);

            entity.Property(e => e.Role)
                .HasConversion<string>()
                .HasMaxLength(32);

            entity.Property(e => e.Content).IsRequired();

            entity.HasIndex(e => e.ConversationId);
        });

        modelBuilder.Entity<Prompt>(entity =>
        {
            ConfigureBaseEntity(entity);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);

            entity.HasIndex(e => e.Name).IsUnique();

            entity.HasMany(e => e.Versions)
                .WithOne(e => e.Prompt)
                .HasForeignKey(e => e.PromptId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PromptVersion>(entity =>
        {
            ConfigureBaseEntity(entity);

            entity.Property(e => e.Content).IsRequired();

            entity.HasIndex(e => new { e.PromptId, e.VersionNumber }).IsUnique();
            entity.HasIndex(e => new { e.PromptId, e.IsPublished });
        });
    }
}
