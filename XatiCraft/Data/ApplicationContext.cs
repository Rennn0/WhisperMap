using Microsoft.EntityFrameworkCore;
using XatiCraft.Model;

namespace XatiCraft.Data;

public partial class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductMetadata> ProductMetadata { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("products_pkey");

            entity.ToTable("products");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Price)
                .HasPrecision(6, 3)
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}