using BulkOps.Api.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BulkOps.Api.Data;

public class BulkOpsDbContext(DbContextOptions<BulkOpsDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public new DbSet<User> Users => Set<User>();

    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();

    public DbSet<Office> Offices => Set<Office>();

    public DbSet<UserOffice> UserOffices => Set<UserOffice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Office>(entity =>
        {
            entity.ToTable("Offices");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.City).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ExternalId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(255).IsRequired();
            entity.Property(x => x.IdentityId).HasMaxLength(450);

            entity.HasOne(x => x.Identity)
                .WithOne(x => x.User)
                .HasForeignKey<User>(x => x.IdentityId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

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

