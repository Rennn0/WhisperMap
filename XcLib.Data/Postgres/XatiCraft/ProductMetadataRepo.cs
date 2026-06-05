using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;
using XcLib.Data.Postgres.XatiCraft.Context;
using XcLib.Data.Postgres.XatiCraft.Model;

namespace XcLib.Data.Postgres.XatiCraft;

public class ProductMetadataRepo : RootRepo<ProductMetadataModel>, IProductMetadaRepo
{
    /// <inheritdoc />
    public ProductMetadataRepo(IDbContextFactory<ApplicationContext> dbContextFactory) : base(dbContextFactory)
    {
    }

    public async Task<ProductMetadata> AddAsync(ProductMetadata obj,
        CancellationToken token = default)
        => await ExecuteAsync(async (productMetadata, cancellationToken) =>
        {
            ProductMetadataModel mpt = new ProductMetadataModel
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

    public async Task<ProductMetadata> GetByIdAsync(long id, CancellationToken token = default) =>
        await ExecuteAsync(
            async (productMetadata, cancellationToken) =>
                ProductMetadata.From(
                    await productMetadata.SingleAsync(pm => pm.Id == id, cancellationToken)), token);

    public async Task<List<ProductMetadata>> GetAsync(ProductMetadata obj,
        sbyte searchFlag = 0,
        CancellationToken token = default) =>
        await ExecuteAsync(async (productMetadata, cancellationToken) =>
        {
            List<ProductMetadataModel> entities = await productMetadata.Where(ToSearchPredicate(obj, searchFlag))
                .ToListAsync(cancellationToken);
            return entities.Select(ProductMetadata.From).ToList();
        }, token);

    public async Task<ProductMetadata?> UpdateAsync(ProductMetadata obj,
        sbyte searchFlag = 0,
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

    public async Task<int> DeleteAsync(ProductMetadata obj, sbyte searchFlag = 0,
        CancellationToken token = default) =>
        await ExecuteTransactionAsync(
            (context, cancellationToken) => context.ProductMetadata
                .Where(ToSearchPredicate(obj, searchFlag))
                .ExecuteDeleteAsync(cancellationToken),
            token: token);

    public async Task<bool> ExistsAsync(ProductMetadata obj, sbyte searchFlag = 0,
        CancellationToken token = default) =>
        await ExecuteAsync(
            (productMetadata, cancellationToken) => productMetadata.AsNoTracking()
                .AnyAsync(ToSearchPredicate(obj, searchFlag), cancellationToken), token);

    protected override Expression<Func<ProductMetadataModel, bool>> ToSearchPredicate(ApplicationObject obj,
        sbyte searchFlag)
    {
        if (obj is not ProductMetadata pmObj) throw new ArgumentOutOfRangeException(nameof(obj));

        return searchFlag switch
        {
            1 => pm => pm.ProductId == pmObj.ProductId,
            _ => pm => pm.Id == pmObj.Id
        };
    }
}