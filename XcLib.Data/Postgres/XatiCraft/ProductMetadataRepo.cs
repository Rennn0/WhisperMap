using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;
using XcLib.Data.Postgres.XatiCraft.Context;
using ProductMetadata = XcLib.Data.Postgres.XatiCraft.Model.ProductMetadata;

namespace XcLib.Data.Postgres.XatiCraft;

public class ProductMetadataRepo : RootRepo<ProductMetadata>, IProductMetadaRepo
{
    /// <inheritdoc />
    public ProductMetadataRepo(IDbContextFactory<ApplicationContext> dbContextFactory) : base(dbContextFactory)
    {
    }

    public async Task<ApplicationObjects.ProductMetadata> AddAsync(ApplicationObjects.ProductMetadata obj,
        CancellationToken token = default)
        => await ExecuteAsync(async (productMetadata, cancellationToken) =>
        {
            ProductMetadata mpt = new ProductMetadata
            {
                FileKey = obj.FileKey,
                Location = obj.Location,
                OriginalFile = obj.OriginalFile,
                ProductId = obj.ProductId,
                Order = obj.Order
            };

            await productMetadata.AddAsync(mpt, cancellationToken);
            obj.Id = mpt.Id;
            return obj;
        }, token);

    public async Task<ApplicationObjects.ProductMetadata> GetByIdAsync(long id, CancellationToken token = default) =>
        await ExecuteAsync(
            async (productMetadata, cancellationToken) =>
                ApplicationObjects.ProductMetadata.From(
                    await productMetadata.SingleAsync(pm => pm.Id == id, cancellationToken)), token);

    public async Task<List<ApplicationObjects.ProductMetadata>> GetAsync(ApplicationObjects.ProductMetadata obj,
        ushort searchFlag = 0,
        CancellationToken token = default) =>
        await ExecuteAsync(async (productMetadata, cancellationToken) =>
        {
            List<ProductMetadata> entities = await productMetadata.Where(ToSearchPredicate(obj, searchFlag))
                .ToListAsync(cancellationToken);
            return entities.Select(ApplicationObjects.ProductMetadata.From).ToList();
        }, token);

    public async Task<ApplicationObjects.ProductMetadata?> UpdateAsync(ApplicationObjects.ProductMetadata obj,
        ushort searchFlag = 0,
        CancellationToken token = default)
    {
        await ExecuteTransactionAsync((context, cancellationToken) => context.ProductMetadata
                .Where(ToSearchPredicate(obj, searchFlag))
                .ExecuteUpdateAsync(calls =>
                        calls
                            .SetProperty(pm => pm.OriginalFile, obj.OriginalFile)
                            .SetProperty(pm => pm.ProductId, obj.ProductId)
                            .SetProperty(pm => pm.FileKey, obj.FileKey)
                            .SetProperty(pm => pm.Location, obj.Location)
                            .SetProperty(pm => pm.Order, obj.Order)
                    , cancellationToken)
            , token: token);
        return obj;
    }

    public async Task<int> DeleteAsync(ApplicationObjects.ProductMetadata obj, ushort searchFlag = 0,
        CancellationToken token = default) =>
        await ExecuteTransactionAsync(
            (context, cancellationToken) => context.ProductMetadata
                .Where(ToSearchPredicate(obj, searchFlag))
                .ExecuteDeleteAsync(cancellationToken),
            token: token);

    public async Task<bool> ExistsAsync(ApplicationObjects.ProductMetadata obj, ushort searchFlag = 0,
        CancellationToken token = default) =>
        await ExecuteAsync(
            (productMetadata, cancellationToken) => productMetadata.AsNoTracking()
                .AnyAsync(ToSearchPredicate(obj, searchFlag), cancellationToken), token);

    protected override Expression<Func<ProductMetadata, bool>> ToSearchPredicate(ApplicationObject obj,
        ushort searchFlag)
    {
        if (obj is not ApplicationObjects.ProductMetadata pmObj) throw new ArgumentOutOfRangeException(nameof(obj));

        return searchFlag switch
        {
            1 => pm => pm.ProductId == pmObj.ProductId,
            _ => pm => pm.Id == pmObj.Id
        };
    }
}