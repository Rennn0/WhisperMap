using Microsoft.EntityFrameworkCore;
using Realtime.Entities;
using ZNetCS.AspNetCore.Logging.EntityFrameworkCore;

namespace Realtime.Context;

public partial class RealtimeDbContext : DbContext
{
    public RealtimeDbContext()
    {
    }

    public RealtimeDbContext(DbContextOptions<RealtimeDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<RealtimeCache> RealtimeCaches { get; set; }
    public virtual DbSet<Log> Logs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => base.OnConfiguring(optionsBuilder);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        LogModelBuilderHelper.Build(modelBuilder.Entity<Log>());
        modelBuilder.Entity<Log>().ToTable("Logs");

        modelBuilder.Entity<RealtimeCache>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Realtime__3214EC077108BA1A");

            entity.Property(e => e.Id).UseCollation("SQL_Latin1_General_CP1_CS_AS");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}