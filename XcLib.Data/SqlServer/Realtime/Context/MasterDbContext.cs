using Microsoft.EntityFrameworkCore;
using XcLib.Data.SqlServer.Realtime.Entities;

namespace XcLib.Data.SqlServer.Realtime.Context;

public partial class MasterDbContext : DbContext
{
    public MasterDbContext(DbContextOptions<MasterDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<MasterLog> MasterLogs { get; set; }
    public virtual DbSet<RealtimeLog> RealtimeLogs { get; set; }
    public virtual DbSet<XaticraftLog> XatiCraftLogs { get; set; } 

    public virtual DbSet<RealtimeCache> RealtimeCaches { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RealtimeCache>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Realtime__3214EC077108BA1A");

            entity.Property(e => e.Id).UseCollation("SQL_Latin1_General_CP1_CS_AS");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
