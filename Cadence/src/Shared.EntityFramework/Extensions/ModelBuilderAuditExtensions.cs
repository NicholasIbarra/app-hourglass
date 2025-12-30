using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shared.EntityFramework.Extensions;

public static class ModelBuilderAuditExtensions
{

    /// <summary>
    /// Adds created audit fields only (CreatedAt, CreatedBy).
    /// </summary>
    public static EntityTypeBuilder<TEntity> ConfigureCreatedAudit<TEntity>(
        this EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        builder.Property<DateTime>(AuditShadowProperties.CreatedAt);
        builder.Property<string?>(AuditShadowProperties.CreatedBy).HasMaxLength(100);

        return builder;
    }

    /// <summary>
    /// Adds created + updated audit fields.
    /// </summary>
    public static EntityTypeBuilder<TEntity> ConfigureUpdatedAudit<TEntity>(
        this EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        builder.Property<DateTime?>(AuditShadowProperties.UpdatedAt);
        builder.Property<string?>(AuditShadowProperties.UpdatedBy).HasMaxLength(100);

        return builder;
    }

    /// <summary>
    /// Adds created + soft delete audit fields and a global query filter.
    /// </summary>
    public static EntityTypeBuilder<TEntity> ConfigureDeletedAudit<TEntity>(
        this EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        builder.Property<bool>(AuditShadowProperties.IsDeleted);
        builder.Property<DateTime?>(AuditShadowProperties.DeletedAt);
        builder.Property<string?>(AuditShadowProperties.DeletedBy).HasMaxLength(100);

        builder.HasQueryFilter(
            e => EF.Property<bool>(e, AuditShadowProperties.IsDeleted) == false
        );

        return builder;
    }

    /// <summary>
    /// Adds full audit fields: created, updated, and soft delete.
    /// </summary>
    public static EntityTypeBuilder<TEntity> ConfigureFullAudit<TEntity>(
        this EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        builder
            .ConfigureCreatedAudit()
            .ConfigureUpdatedAudit()
            .ConfigureDeletedAudit();

        return builder;
    }

    public static EntityTypeBuilder<TEntity> ConfigureCreatedUpdatedAudit<TEntity>(
        this EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        builder
            .ConfigureCreatedAudit()
            .ConfigureUpdatedAudit();

        return builder;
    }

    public static EntityTypeBuilder<TEntity> ConfigureCreatedDeletedAudit<TEntity>(
        this EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        builder
            .ConfigureCreatedAudit()
            .ConfigureDeletedAudit();

        return builder;
    }
}
