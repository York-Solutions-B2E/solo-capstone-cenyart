using Microsoft.EntityFrameworkCore;
using WebApi.Data.Entities;

namespace WebApi.Data;
public class CommunicationDbContext(DbContextOptions<CommunicationDbContext> options) : DbContext(options)
{
    public DbSet<GlobalStatus> GlobalStatuses { get; set; }
    public DbSet<CommunicationType> CommunicationTypes { get; set; }
    public DbSet<CommunicationTypeStatus> CommunicationTypeStatuses { get; set; }
    public DbSet<Communication> Communications { get; set; }
    public DbSet<CommunicationStatusHistory> CommunicationStatusHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // GlobalStatus configuration
        modelBuilder.Entity<GlobalStatus>(entity =>
        {
            entity.HasKey(e => e.StatusCode);
            entity.Property(e => e.StatusCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Phase).HasMaxLength(20).IsRequired()
                .HasConversion<string>(); // Enum to string conversion
            entity.Property(e => e.SortOrder).IsRequired();
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
            entity.Property(e => e.CreatedUtc).IsRequired().HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => new { e.Phase, e.SortOrder });
            entity.HasIndex(e => e.IsActive);
        });

        // CommunicationType configuration
        modelBuilder.Entity<CommunicationType>(entity =>
        {
            entity.HasKey(e => e.TypeCode);
            entity.Property(e => e.TypeCode).HasMaxLength(20).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
            entity.Property(e => e.CreatedUtc).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.ModifiedUtc).IsRequired().HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.DisplayName);
        });

        // CommunicationTypeStatus configuration
        modelBuilder.Entity<CommunicationTypeStatus>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TypeCode).HasMaxLength(20).IsRequired();
            entity.Property(e => e.StatusCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.SortOrder).IsRequired().HasDefaultValue(0);
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);

            entity.HasOne(e => e.CommunicationType)
                .WithMany(ct => ct.ValidStatuses)
                .HasForeignKey(e => e.TypeCode)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.GlobalStatus)
                .WithMany()
                .HasForeignKey(e => e.StatusCode)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.TypeCode, e.StatusCode }).IsUnique();
            entity.HasIndex(e => e.IsActive);
        });

        // Communication configuration
        modelBuilder.Entity<Communication>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.TypeCode).HasMaxLength(20).IsRequired();
            entity.Property(e => e.CurrentStatus).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SourceFileUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedUtc).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.LastUpdatedUtc).IsRequired().HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.CommunicationType)
                .WithMany()
                .HasForeignKey(e => e.TypeCode)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CurrentStatusNavigation)
                .WithMany()
                .HasForeignKey(e => e.CurrentStatus)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.TypeCode);
            entity.HasIndex(e => e.CurrentStatus);
            entity.HasIndex(e => e.LastUpdatedUtc);
        });

        // CommunicationStatusHistory configuration
        modelBuilder.Entity<CommunicationStatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CommunicationId).IsRequired();
            entity.Property(e => e.StatusCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.OccurredUtc).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.EventData).HasColumnType("NVARCHAR(MAX)");

            entity.HasOne(e => e.Communication)
                .WithMany(c => c.StatusHistory)
                .HasForeignKey(e => e.CommunicationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Status)
                .WithMany()
                .HasForeignKey(e => e.StatusCode)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.CommunicationId);
            entity.HasIndex(e => e.OccurredUtc);
        });
    }
}
