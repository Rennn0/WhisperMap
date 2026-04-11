using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using XcLib.Data.Abstractions;
using XcLib.Data.Postgres.XatiCraft.Context;
using XcLib.Data.Postgres.XatiCraft.Model;
using Product = XcLib.Data.ApplicationObjects.Product;
using ProductMetadata = XcLib.Data.ApplicationObjects.ProductMetadata;

namespace XcLib.Data.Postgres.XatiCraft;

/// <inheritdoc />
public class ProductRepo : IProductRepo
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
    public async Task<List<Product>> SelectAsync(
        IEnumerable<long>? ids = null,
        OrderBy? orderBy = null,
        string? query = null,
        SearchCursor? cursor = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cursor);
        
        OrderBy order = orderBy ?? OrderBy.NewestFirst;
        IQueryable<VProduct> dbQuery = _context.VProducts.AsNoTracking();

        if (ids is not null)
        {
            List<long> pIds = ids.Distinct().ToList();
            dbQuery = dbQuery.Where(x => pIds.Contains(x.Id ?? -1));
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            string q = query.Trim();
            bool queryPrice = decimal.TryParse(q, out _);
            dbQuery = dbQuery.Where(x =>
                (x.Title != null && EF.Functions.ILike(x.Title, $"%{q}%")) ||
                (x.Description != null && EF.Functions.ILike(x.Description, $"%{q}%")) ||
                (x.Price.HasValue && queryPrice && Math.Abs(x.Price.Value - decimal.Parse(q)) <= 5));
        }

        dbQuery = ApplyCursor(dbQuery, order, cursor);
        dbQuery = ApplyOrdering(dbQuery, order);

        List<Product> result = await dbQuery
            .Take((int)cursor.BatchSize)
            .Select(v =>
                new Product(v.Title ?? "", v.Description ?? "", v.Price ?? 0m)
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
    public async Task<Product?> SelectAsync(long id, CancellationToken cancellationToken)
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

    public Task<Product?> SelectAsync(string objId, CancellationToken cancellationToken) => throw new NotImplementedException();

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

    public async Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(product.Id);

        int affected = await _context.Products
            .Where(p=>p.Id == product.Id)
            .ExecuteUpdateAsync(calls => calls
                .SetProperty(p=>p.Description,product.Description)
                .SetProperty(p=>p.Title,product.Title)
                .SetProperty(p=>p.Price,product.Price), cancellationToken);

        return product;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken) => await _context.Products.AsNoTracking().AnyAsync(p => p.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(string objId, CancellationToken cancellationToken) => throw new NotImplementedException();

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken)
    {
        Model.Product entity = new Model.Product { Id = id };
        _context.Products.Attach(entity);
        _context.Products.Remove(entity);
        int affected = await _context.SaveChangesAsync(cancellationToken);
        return affected > 0;
    }

    public Task<bool> DeleteAsync(string objId, CancellationToken cancellationToken) => throw new NotImplementedException();

    private static IQueryable<VProduct> ApplyOrdering(IQueryable<VProduct> queryable, OrderBy? orderBy) =>
        orderBy switch
        {
            OrderBy.NewestFirst => queryable.OrderByDescending(x => x.Timestamp).ThenByDescending(x => x.Id),
            OrderBy.OldestFirst => queryable.OrderBy(x => x.Timestamp).ThenBy(x => x.Id),
            OrderBy.PriceIncreasing => queryable.OrderBy(x => x.Price).ThenBy(x => x.Id),
            OrderBy.PriceDecreasing => queryable.OrderByDescending(x => x.Price).ThenByDescending(x => x.Id),
            null => queryable.OrderByDescending(x => x.Timestamp)
                .ThenByDescending(x => x.Id),
            _ => throw new ArgumentOutOfRangeException(nameof(orderBy), orderBy, null)
        };

    private static IQueryable<VProduct> ApplyCursor(IQueryable<VProduct> queryable, OrderBy orderBy,
        SearchCursor? cursor)
    {
        if (cursor is not { Id: not null } searchCursor) return queryable;

        return orderBy switch
        {
            OrderBy.NewestFirst => queryable.Where(x =>
                x.Timestamp < searchCursor.Timestamp || (x.Timestamp == searchCursor.Timestamp && x.Id  < searchCursor.Id )),
            OrderBy.OldestFirst => queryable.Where(x =>
                x.Timestamp > searchCursor.Timestamp || (x.Timestamp == searchCursor.Timestamp && x.Id  > searchCursor.Id )),
            OrderBy.PriceIncreasing => queryable.Where(x =>
                x.Price  > searchCursor.Price || (x.Price  == searchCursor.Price && x.Id  > searchCursor.Id )),
            OrderBy.PriceDecreasing => queryable.Where(x =>
                x.Price  < searchCursor.Price || (x.Price  == searchCursor.Price && x.Id  < searchCursor.Id )),
            _ => throw new ArgumentOutOfRangeException(nameof(orderBy), orderBy, null)
        };
    }
}