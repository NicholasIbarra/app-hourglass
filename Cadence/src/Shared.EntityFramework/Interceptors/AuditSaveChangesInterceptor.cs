using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.EntityFramework.Extensions;

namespace Shared.EntityFramework.Interceptors
{
    public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            ApplyAuditing(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            ApplyAuditing(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void ApplyAuditing(DbContext? context)
        {
            if (context == null)
                return;

            var now = DateTime.UtcNow;
            var username = "system"; // _currentUserProvider.UserId;

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added)
                {
                    SetIfExists(entry, AuditShadowProperties.CreatedAt, now);
                    SetIfExists(entry, AuditShadowProperties.CreatedBy, username);
                }

                if (entry.State == EntityState.Modified)
                {
                    SetIfExists(entry, AuditShadowProperties.UpdatedAt, now);
                    SetIfExists(entry, AuditShadowProperties.UpdatedBy, username);
                }

                if (entry.State == EntityState.Deleted)
                {
                    if (!HasProperty(entry, AuditShadowProperties.IsDeleted))
                        continue;

                    // Convert hard delete → soft delete
                    entry.State = EntityState.Modified;

                    SetIfExists(entry, AuditShadowProperties.IsDeleted, true);
                    SetIfExists(entry, AuditShadowProperties.DeletedAt, now);
                    SetIfExists(entry, AuditShadowProperties.DeletedBy, username);
                }
            }
        }

        private static bool HasProperty(EntityEntry entry, string propertyName)
            => entry.Metadata.FindProperty(propertyName) != null;

        private static void SetIfExists(
            EntityEntry entry,
            string propertyName,
            object? value)
        {
            if (!HasProperty(entry, propertyName))
                return;

            entry.Property(propertyName).CurrentValue = value;
        }
    }

}
