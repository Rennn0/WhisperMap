using Microsoft.EntityFrameworkCore;
using WhisperMap.Models;
using Object = WhisperMap.Models.Object;

namespace WhisperMap.Data;

public partial class MapContext : DbContext
{
    public MapContext(DbContextOptions<MapContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Object> Objects { get; set; }

    public virtual DbSet<Pedestrian> Pedestrians { get; set; }

    public virtual DbSet<Pin> Pins { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("fuzzystrmatch")
            .HasPostgresExtension("postgis")
            .HasPostgresExtension("tiger", "postgis_tiger_geocoder")
            .HasPostgresExtension("topology", "postgis_topology");

        modelBuilder.Entity<Object>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("object_pkey");

            entity.HasOne(d => d.Pin).WithOne(p => p.Object)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("object_pinid_fkey");
        });

        modelBuilder.Entity<Pedestrian>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pedestrian_pkey");

            entity.Property(e => e.Createdat).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Pin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pin_pkey");

            entity.Property(e => e.Createdat).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Pedestrian).WithMany(p => p.Pins).HasConstraintName("pin_pedestrianid_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
