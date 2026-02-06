using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using XatiCraft.Data.Objects;

namespace XatiCraft.Data.Repos.EfCoreImpl;

/// <inheritdoc />
internal class ProductRepo : IProductRepo
{
    private readonly ApplicationContext _context;
    private readonly JsonSerializerOptions _serializerOptions;

    /// <summary>
    ///     implementation using Npgsql.EntityFrameworkCore.PostgreSQL Version=8.0.0
    /// </summary>
    /// <param name="context"></param>
    public ProductRepo(ApplicationContext context)
    {
        _context = context;
        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = new SnakeCaseSerializer()
        };
    }

    /// <inheritdoc />
    public async Task<List<Product>> SelectProductsAsync(CancellationToken cancellationToken)
    {
        List<Product> result = await _context.VProducts.AsNoTracking()
            .Select(v => new Product(v.Title ?? "", v.Description ?? "", v.Price ?? 0m)
            {
                Id = v.Id,
                Timestamp = v.Timestamp,
                ProductMetadata = v.Metadata == null
                    ? null
                    : v.Metadata.Deserialize<ICollection<ProductMetadata>>(_serializerOptions)
            })
            .ToListAsync(cancellationToken);

        return result;
    }

    /// <inheritdoc />
    public async Task<Product?> SelectProductAsync(long id, CancellationToken cancellationToken)
    {
        Product? result = await _context.VProducts.AsNoTracking()
            .Where(vp => vp.Id == id)
            .Select(v => new Product(v.Title ?? "", v.Description ?? "", v.Price ?? 0m)
            {
                Id = v.Id,
                Timestamp = v.Timestamp,
                ProductMetadata = v.Metadata == null
                    ? null
                    : v.Metadata.Deserialize<ICollection<ProductMetadata>>(_serializerOptions)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return result;
    }

    /// <inheritdoc />
    public async Task<Product> InsertAsync(Product product, CancellationToken cancellationToken)
    {
        Model.Product mp = new Model.Product
        {
            Description = product.Description,
            Title = product.Title,
            Price = product.Price
        };
        await _context.Products.AddAsync(mp, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        product.Id = mp.Id;
        product.Timestamp = mp.Timestamp;
        return product;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken)
    {
        return await _context.Products.AsNoTracking().AnyAsync(p => p.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken)
    {
        Model.Product entity = new Model.Product { Id = id };
        _context.Products.Attach(entity);
        _context.Products.Remove(entity);
        int affected = await _context.SaveChangesAsync(cancellationToken);
        return affected > 0;
    }
}