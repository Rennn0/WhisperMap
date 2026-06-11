using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using XcLib.Data.Postgres.XatiCraft.Model;

namespace XcLib.Data.Postgres.XatiCraft.Context;

/// <summary>
/// </summary>
public partial class ApplicationContext : DbContext, IDataProtectionKeyContext
{
    /// <inheritdoc />
    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// </summary>
    public virtual DbSet<ProductModel> Products { get; set; }

    /// <summary>
    /// </summary>
    public virtual DbSet<ProductMetadataModel> ProductMetadata { get; set; }

    /// <summary>
    /// </summary>
    public virtual DbSet<VProductModel> VProducts { get; set; }

    /// <summary>
    /// </summary>
    public virtual DbSet<PageVisitorModel> PageVisitors { get; set; }

    /// <summary>
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductModel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("products_pkey");

            entity.ToTable("products");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Price)
                .HasPrecision(10, 3)
                .HasColumnName("price");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("timestamp");
            entity.Property(e => e.Title).HasColumnName("title");
        });

        modelBuilder.Entity<ProductMetadataModel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("product_metadata_pkey");

            entity.ToTable("product_metadata");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FileKey).HasColumnName("file_key");
            entity.Property(e => e.Location).HasColumnName("location");
            entity.Property(e => e.OriginalFile).HasColumnName("original_file");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("timestamp");
            entity.Property(e => e.Order).HasColumnName("order");
            entity.Property(e => e.InStock).HasColumnName("in_stock");

            entity.HasOne(d => d.ProductModel).WithMany(p => p.ProductMetadata)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("product_metadata_product_id_fkey");
        });

        modelBuilder.Entity<VProductModel>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_products");

            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Metadata)
                .HasColumnType("jsonb")
                .HasColumnName("metadata");
            entity.Property(e => e.Price)
                .HasPrecision(10, 3)
                .HasColumnName("price");
            entity.Property(e => e.Timestamp)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("timestamp");
            entity.Property(e => e.Title).HasColumnName("title");
        });

        modelBuilder.Entity<PageVisitorModel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("page_visitors_pkey");
            entity.ToTable("page_visitors");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Page).IsRequired().HasColumnName("page");
            entity.Property(e => e.IpAddress).HasColumnName("ip_address");
            entity.Property(e => e.Uid).HasColumnName("uid");
            entity.Property(e => e.Browser).HasColumnName("browser");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at")
                .HasColumnType("timestamp  with time zone")
                .HasDefaultValueSql("now()");

            entity.HasIndex(e => new { e.Page, e.IpAddress });
        });
        
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    /// <inheritdoc />
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
}