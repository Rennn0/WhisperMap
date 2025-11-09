using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using XatiCraft.Objects;

namespace XatiCraft.Data.Repos.EfCoreImpl;

/// <summary>
/// </summary>
public class ProductRepo : IProductRepo
{
    private readonly ApplicationContext _context;
    private readonly JsonSerializerOptions _serializerOptions;

    /// <summary>
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

    /// <summary>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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

    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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

    /// <summary>
    /// </summary>
    /// <param name="product"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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

    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken)
    {
        return await _context.Products.AsNoTracking().AnyAsync(p => p.Id == id, cancellationToken);
    }
}