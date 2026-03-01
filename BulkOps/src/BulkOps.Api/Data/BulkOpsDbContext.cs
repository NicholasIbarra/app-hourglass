using BulkOps.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace BulkOps.Api.Data;

public class BulkOpsDbContext(DbContextOptions<BulkOpsDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Office> Offices => Set<Office>();

    public DbSet<UserOffice> UserOffices => Set<UserOffice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Office>(entity =>
        {
            entity.ToTable("Offices");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.City).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ExternalId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(255).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasIndex(x => x.ExternalId).IsUnique();

            entity.HasMany(x => x.Offices)
                .WithMany(x => x.Users)
                .UsingEntity<UserOffice>(
                    configureRight: relationship => relationship
                        .HasOne(x => x.Office)
                        .WithMany(x => x.UserOffices)
                        .HasForeignKey(x => x.OfficeId),
                    configureLeft: relationship => relationship
                        .HasOne(x => x.User)
                        .WithMany(x => x.UserOffices)
                        .HasForeignKey(x => x.UserId),
                    configureJoinEntityType: relationship =>
                    {
                        relationship.ToTable("UserOffices");
                        relationship.HasKey(x => new { x.UserId, x.OfficeId });
                        relationship.Property(x => x.AssignedAtUtc)
                            .HasPrecision(0)
                            .HasDefaultValueSql("GETUTCDATE()");
                        relationship.HasIndex(x => x.OfficeId);
                    });
        });
    }
}
