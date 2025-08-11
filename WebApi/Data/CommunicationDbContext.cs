using Microsoft.EntityFrameworkCore;

namespace WebApi.Data;

public class CommunicationDbContext(DbContextOptions<CommunicationDbContext> options) : DbContext(options)
{
    public required DbSet<GlobalStatus> GlobalStatuses { get; set; }
    public required DbSet<Type> Types { get; set; }
    public required DbSet<Status> Statuses { get; set; }
    public required DbSet<Communication> Communications { get; set; }
    public required DbSet<StatusHistory> StatusHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder model)
    {
        base.OnModelCreating(model);

        //
        // Query filters for soft-delete (only on Type and Status)
        //
        model.Entity<Type>().HasQueryFilter(t => t.IsActive);
        model.Entity<Status>().HasQueryFilter(s => s.IsActive);

        // -------------------
        // GLOBAL STATUS
        // -------------------
        model.Entity<GlobalStatus>(e =>
        {
            e.ToTable("GlobalStatuses");
            e.HasKey(g => g.StatusCode);
            e.Property(g => g.StatusCode).HasMaxLength(100).IsRequired();
            e.Property(g => g.Phase).HasMaxLength(100).IsRequired();
            e.Property(g => g.Notes).HasMaxLength(100).IsRequired();

            e.HasIndex(g => g.Phase);
        });

        // -------------------
        // TYPE
        // -------------------
        model.Entity<Type>(e =>
        {
            e.ToTable("Types");
            e.HasKey(t => t.TypeCode);
            e.Property(t => t.TypeCode).HasMaxLength(100).IsRequired();
            e.Property(t => t.DisplayName).HasMaxLength(100).IsRequired();
            e.Property(t => t.IsActive).IsRequired();

            e.HasMany(t => t.ValidStatuses)
             .WithOne(s => s.Type)
             .HasForeignKey(s => s.TypeCode)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(t => t.Communications)
             .WithOne(c => c.Type)
             .HasForeignKey(c => c.TypeCode)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // -------------------
        // STATUS
        // -------------------
        model.Entity<Status>(e =>
        {
            e.ToTable("Statuses");
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).ValueGeneratedOnAdd();
            e.Property(s => s.TypeCode).HasMaxLength(100).IsRequired();
            e.Property(s => s.StatusCode).HasMaxLength(100).IsRequired();
            e.Property(s => s.Description).HasMaxLength(100).IsRequired();
            e.Property(s => s.IsActive).IsRequired();

            // prevent duplicate allowed-status entries
            e.HasIndex(s => new { s.TypeCode, s.StatusCode }).IsUnique();

            e.HasOne(s => s.Type)
             .WithMany(t => t.ValidStatuses)
             .HasForeignKey(s => s.TypeCode)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(s => s.GlobalStatus)
             .WithMany(g => g.StatusLinks)
             .HasForeignKey(s => s.StatusCode)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(s => s.TypeCode);
            e.HasIndex(s => s.StatusCode);
        });

        // -------------------
        // COMMUNICATION
        // -------------------
        model.Entity<Communication>(e =>
        {
            e.ToTable("Communications");
            e.HasKey(c => c.Id);
            e.Property(c => c.Title).HasMaxLength(100).IsRequired();
            e.Property(c => c.TypeCode).HasMaxLength(100).IsRequired();
            e.Property(c => c.CurrentStatusCode).HasMaxLength(100).IsRequired();
            e.Property(c => c.LastUpdatedUtc).IsRequired();

            e.HasOne(c => c.Type)
             .WithMany(t => t.Communications)
             .HasForeignKey(c => c.TypeCode)
             .IsRequired(false)    // optional to handle soft delete
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<GlobalStatus>()
             .WithMany()
             .HasForeignKey(c => c.CurrentStatusCode)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(c => c.StatusHistory)
             .WithOne(h => h.Communication)
             .HasForeignKey(h => h.CommunicationId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(c => c.TypeCode);
            e.HasIndex(c => c.CurrentStatusCode);
        });

        // -------------------
        // STATUS HISTORY
        // -------------------
        model.Entity<StatusHistory>(e =>
        {
            e.ToTable("StatusHistories");
            e.HasKey(h => h.Id);
            e.Property(h => h.Id).ValueGeneratedOnAdd();
            e.Property(h => h.StatusCode).HasMaxLength(100).IsRequired();
            e.Property(h => h.OccurredUtc).IsRequired();

            e.HasOne(h => h.Communication)
             .WithMany(c => c.StatusHistory)
             .HasForeignKey(h => h.CommunicationId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(h => new { h.CommunicationId, h.OccurredUtc });
            e.HasIndex(h => h.StatusCode);
        });
    }
}
