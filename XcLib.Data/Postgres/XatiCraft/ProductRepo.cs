using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;
using XcLib.Data.Postgres.XatiCraft.Context;
using XcLib.Data.Postgres.XatiCraft.Model;
using Product = XcLib.Data.ApplicationObjects.Product;
using ProductMetadata = XcLib.Data.ApplicationObjects.ProductMetadata;

namespace XcLib.Data.Postgres.XatiCraft;

public class ProductRepo : RootRepo<Model.Product>, IProductRepo
{
    private readonly JsonSerializerOptions _serializerOptions;

    /// <inheritdoc />
    public ProductRepo(IDbContextFactory<ApplicationContext> dbContextFactory) : base(dbContextFactory)
    {
        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = new SnakeCaseSerializer()
        };
    }

    /// <inheritdoc />
    public async Task<List<Product>> GetAsync(
        IEnumerable<long>? ids = null,
        OrderBy? orderBy = null,
        string? query = null,
        SearchCursor? cursor = null,
        CancellationToken cancellationToken = default) =>
        await ExecuteTransactionAsync(async (context, token) =>
        {
            ArgumentNullException.ThrowIfNull(cursor);

            OrderBy order = orderBy ?? OrderBy.NewestFirst;
            IQueryable<VProduct> dbQuery = context.VProducts.AsNoTracking();

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
                .ToListAsync(token);

            return result;
        }, token: cancellationToken);

    public async Task<Product> AddAsync(Product obj, CancellationToken token = default) =>
        Product.From(await ExecuteAsync(async (products, cancellationToken) =>
        {
            Model.Product mp = new Model.Product
            {
                Description = obj.Description,
                Title = obj.Title,
                Price = obj.Price
            };
            await products.AddAsync(mp, cancellationToken);
            return mp;
        }, token));

    public async Task<Product> GetByIdAsync(long id, CancellationToken token = default) =>
        await ExecuteTransactionAsync(async (context, cancellationToken) =>
        {
            VProduct v = await context.VProducts.AsNoTracking()
                .SingleAsync(vp => vp.Id == id, cancellationToken);

            return new Product(v.Title ?? "", v.Description ?? "", v.Price ?? 0m)
            {
                Id = v.Id,
                Timestamp = v.Timestamp,
                ProductMetadata = v.Metadata?.Deserialize<ICollection<ProductMetadata>>(_serializerOptions)
            };
        }, token: token);

    public Task<List<Product>> GetAsync(Product obj, ushort searchFlag = 0, CancellationToken token = default) =>
        throw new NotImplementedException();

    public async Task<Product?> UpdateAsync(Product obj, ushort searchFlag = 0, CancellationToken token = default) =>
        await ExecuteTransactionAsync(async (context, cancellationToken) =>
        {
            await context.Products
                .Where(ToSearchPredicate(obj, searchFlag))
                .ExecuteUpdateAsync(calls => calls
                    .SetProperty(p => p.Description, obj.Description)
                    .SetProperty(p => p.Title, obj.Title)
                    .SetProperty(p => p.Price, obj.Price), cancellationToken);

            return obj;
        }, token: token);

    public async Task<int> DeleteAsync(Product obj, ushort searchFlag = 0, CancellationToken token = default) =>
        await ExecuteAsync(
            async (products, cancellationToken) =>
                await products.Where(ToSearchPredicate(obj, searchFlag)).ExecuteDeleteAsync(cancellationToken),
            token);

    public async Task<bool> ExistsAsync(Product obj, ushort searchFlag = 0, CancellationToken token = default) =>
        await ExecuteAsync((products, cancellationToken) =>
                products.AsNoTracking().AnyAsync(ToSearchPredicate(obj, searchFlag), cancellationToken)
            , token);

    public Task<bool> ExistsAsync(string objId, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<bool> DeleteAsync(string objId, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<Product> GetByIdAsync(string objId, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
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

    protected override Expression<Func<Model.Product, bool>> ToSearchPredicate(ApplicationObject obj, ushort searchFlag)
    {
        if (obj is not Product pObj) throw new ArgumentOutOfRangeException(nameof(obj));
        return searchFlag switch
        {
            _ => p => p.Id == pObj.Id
        };
    }
}