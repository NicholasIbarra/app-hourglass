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
                var name = entry.Metadata.Name;

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
                    // When an owned value object (owned entity type) is marked as Deleted due to principal soft-delete,
                    // ensure it isn't physically deleted or nulled out. Mark it as Unchanged and skip auditing.
                    if (entry.Metadata.IsOwned())
                    {
                        entry.State = EntityState.Unchanged;
                        continue;
                    }

                    if (!HasProperty(entry, AuditShadowProperties.IsDeleted))
                        continue;

                    // Switch to unchanged to avoid EF cascading delete/nulling while still updating shadow props.
                    entry.State = EntityState.Unchanged;

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

            var propEntry = entry.Property(propertyName);

            propEntry.CurrentValue = value;
            propEntry.IsModified = true;


        }
    }

}
