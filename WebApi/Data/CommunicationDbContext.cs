using Microsoft.EntityFrameworkCore;

namespace WebApi.Data;

public class CommunicationDbContext(DbContextOptions<CommunicationDbContext> options) : DbContext(options)
{
    public DbSet<CommunicationType>             CommunicationTypes         { get; set; }
    public DbSet<CommunicationTypeStatus>       CommunicationTypeStatuses  { get; set; }
    public DbSet<Communication>                 Communications             { get; set; }
    public DbSet<CommunicationStatusHistory>    CommunicationStatusHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder model)
    {
        base.OnModelCreating(model);

        // Soft‐delete filters
        model.Entity<CommunicationType>().HasQueryFilter(e => e.IsActive);
        model.Entity<CommunicationTypeStatus>().HasQueryFilter(e => e.IsActive);
        model.Entity<Communication>().HasQueryFilter(e => e.IsActive);
        model.Entity<CommunicationStatusHistory>().HasQueryFilter(e => e.IsActive);

        // CommunicationType
        model.Entity<CommunicationType>(e =>
        {
            e.HasKey(x => x.TypeCode);
            e.Property(x => x.DisplayName).IsRequired();
            e.HasMany(x => x.ValidStatuses)
             .WithOne(s => s.CommunicationType)
             .HasForeignKey(s => s.TypeCode)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Communications)
             .WithOne(c => c.CommunicationType)
             .HasForeignKey(c => c.TypeCode)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // CommunicationTypeStatus
        model.Entity<CommunicationTypeStatus>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.StatusCode).IsRequired();
            e.Property(x => x.Description).IsRequired();
            e.Property(x => x.TypeCode).IsRequired();
            e.HasIndex(x => x.TypeCode);
        });

        // Communication
        model.Entity<Communication>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired();
            e.Property(x => x.TypeCode).IsRequired();
            e.Property(x => x.CurrentStatus).IsRequired();
            e.Property(x => x.LastUpdatedUtc).IsRequired();
            e.HasOne(c => c.CommunicationType)
             .WithMany(t => t.Communications)
             .HasForeignKey(c => c.TypeCode)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasMany(c => c.StatusHistory)
             .WithOne(h => h.Communication)
             .HasForeignKey(h => h.CommunicationId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.TypeCode);
        });

        // CommunicationStatusHistory
        model.Entity<CommunicationStatusHistory>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.StatusCode).IsRequired();
            e.Property(x => x.OccurredUtc).IsRequired();
            e.HasOne(h => h.Communication)
             .WithMany(c => c.StatusHistory)
             .HasForeignKey(h => h.CommunicationId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(h => new { h.CommunicationId, h.OccurredUtc });
        });
    }
}
