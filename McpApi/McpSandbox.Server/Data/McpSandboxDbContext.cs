using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using McpSandbox.Server.Domain.Entities.Availabilities;
using McpSandbox.Server.Domain.Entities.Availabilities.ValueObjects;
using McpSandbox.Server.Domain.Entities.Offices;
using McpSandbox.Server.Domain.Entities.ShiftRequests;
using McpSandbox.Server.Domain.Entities.Users;

namespace McpSandbox.Server.Data;

public sealed class McpSandboxDbContext : DbContext
{
    public McpSandboxDbContext(DbContextOptions<McpSandboxDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Office> Offices => Set<Office>();

    public DbSet<Availability> Availabilities => Set<Availability>();

    public DbSet<Unavailability> Unavailabilities => Set<Unavailability>();

    public DbSet<ShiftRequest> ShiftRequests => Set<ShiftRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        static void ConfigureBaseEntity<TEntity>(EntityTypeBuilder<TEntity> entity)
            where TEntity : McpSandbox.Server.Domain.Entities.BaseEntity
        {
            entity.HasKey(e => e.Id);

            // Entities in this solution generate their keys client-side.
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(e => e.UpdatedAt)
                .IsRequired(false);
        }

        modelBuilder.Entity<User>(entity =>
        {
            ConfigureBaseEntity(entity);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(320);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.TimeZone).HasMaxLength(100);
            entity.Property(e => e.Locale).HasMaxLength(20);
            entity.Property(e => e.AvatarUrl).HasMaxLength(2048);

            // Store enums as strings for readability.
            entity.Property(e => e.Type)
                .HasConversion<string>()
                .HasMaxLength(32);

            entity.HasIndex(e => e.Email);

            // Many-to-many User <-> Office (implicit join table)
            entity.HasMany(e => e.Offices)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "UserOffice",
                    join => join.HasOne<Office>().WithMany().HasForeignKey("OfficeId"),
                    join => join.HasOne<User>().WithMany().HasForeignKey("UserId"),
                    join => join.HasKey("UserId", "OfficeId"));
        });

        modelBuilder.Entity<Office>(entity =>
        {
            ConfigureBaseEntity(entity);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.TimeZone).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(4000);
            entity.Property(e => e.AddressLine1).HasMaxLength(200);
            entity.Property(e => e.AddressLine2).HasMaxLength(200);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.StateOrProvince).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Country).HasMaxLength(100);

            entity.HasIndex(e => e.Code);
        });

        // Store AvailabilityDayTimeRange as a compact string: "{dayOfWeek}|{HH:mm}|{HH:mm}".
        // This avoids EF needing to materialize a get-only record struct.
        var availabilityDayTimeRangeConverter = new ValueConverter<AvailabilityDayTimeRange?, string?>(
            v => v == null
                ? null
                : $"{(int)v.Value.DayOfWeek}|{v.Value.StartTime:HH\\:mm}|{v.Value.EndTime:HH\\:mm}",
            v => string.IsNullOrWhiteSpace(v)
                ? null
                : ParseAvailabilityDayTimeRange(v));

        modelBuilder.Entity<Availability>(entity =>
        {
            ConfigureBaseEntity(entity);

            entity.Property(e => e.EffectiveFrom).HasColumnType("date");
            entity.Property(e => e.EffectiveTo).HasColumnType("date");

            entity.Property(e => e.Sunday).HasConversion(availabilityDayTimeRangeConverter).HasMaxLength(32);
            entity.Property(e => e.Monday).HasConversion(availabilityDayTimeRangeConverter).HasMaxLength(32);
            entity.Property(e => e.Tuesday).HasConversion(availabilityDayTimeRangeConverter).HasMaxLength(32);
            entity.Property(e => e.Wednesday).HasConversion(availabilityDayTimeRangeConverter).HasMaxLength(32);
            entity.Property(e => e.Thursday).HasConversion(availabilityDayTimeRangeConverter).HasMaxLength(32);
            entity.Property(e => e.Friday).HasConversion(availabilityDayTimeRangeConverter).HasMaxLength(32);
            entity.Property(e => e.Saturday).HasConversion(availabilityDayTimeRangeConverter).HasMaxLength(32);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId);

            // Many-to-many Availability <-> Office (implicit join table)
            entity.HasMany(e => e.Offices)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "AvailabilityOffice",
                    join => join.HasOne<Office>().WithMany().HasForeignKey("OfficeId"),
                    join => join.HasOne<Availability>().WithMany().HasForeignKey("AvailabilityId"),
                    join => join.HasKey("AvailabilityId", "OfficeId"));

            entity.HasIndex(e => new { e.UserId, e.EffectiveFrom });
        });

        modelBuilder.Entity<Unavailability>(entity =>
        {
            ConfigureBaseEntity(entity);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId);

            entity.Property(e => e.StartDate).HasColumnType("datetime2");
            entity.Property(e => e.EndDate).HasColumnType("datetime2");

            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(4000);

            entity.HasIndex(e => e.UserId);
        });

        modelBuilder.Entity<ShiftRequest>(entity =>
        {
            ConfigureBaseEntity(entity);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId);

            entity.HasOne(e => e.Office)
                .WithMany()
                .HasForeignKey(e => e.OfficeId);

            // Store enums as strings for readability.
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(32);

            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(4000);

            entity.HasIndex(e => new { e.UserId, e.StartAt });
            entity.HasIndex(e => new { e.OfficeId, e.StartAt });
        });
    }

    private static AvailabilityDayTimeRange ParseAvailabilityDayTimeRange(string value)
    {
        // "{dayOfWeek}|{HH:mm}|{HH:mm}"
        var parts = value.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
        {
            throw new FormatException($"Invalid {nameof(AvailabilityDayTimeRange)} value: '{value}'.");
        }

        var dayOfWeek = (DayOfWeek)int.Parse(parts[0]);
        var start = TimeOnly.Parse(parts[1]);
        var end = TimeOnly.Parse(parts[2]);

        return new AvailabilityDayTimeRange(dayOfWeek, start, end);
    }
}
