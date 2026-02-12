using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using XatiCraft.Data.Repos.EfCoreImpl.Model;

namespace XatiCraft.Data.Repos.EfCoreImpl;

/// <inheritdoc />
public partial class ApplicationContext : DbContext, IDataProtectionKeyContext
{
    /// <inheritdoc />
    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// </summary>
    public virtual DbSet<Product> Products { get; set; }

    /// <summary>
    /// </summary>
    public virtual DbSet<ProductMetadata> ProductMetadata { get; set; }

    /// <summary>
    /// </summary>
    public virtual DbSet<VProduct> VProducts { get; set; }

    /// <summary>
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
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

        modelBuilder.Entity<ProductMetadata>(entity =>
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

            entity.HasOne(d => d.Product).WithMany(p => p.ProductMetadata)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("product_metadata_product_id_fkey");
        });

        modelBuilder.Entity<VProduct>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_products");

            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Metadata)
                // .HasColumnType("json")
                .HasColumnName("metadata");
            entity.Property(e => e.Price)
                .HasPrecision(10, 3)
                .HasColumnName("price");
            entity.Property(e => e.Timestamp)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("timestamp");
            entity.Property(e => e.Title).HasColumnName("title");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    /// <inheritdoc />
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
}