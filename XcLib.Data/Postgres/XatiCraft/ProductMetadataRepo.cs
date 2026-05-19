using Microsoft.EntityFrameworkCore;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;
using XcLib.Data.Postgres.XatiCraft.Context;

namespace XcLib.Data.Postgres.XatiCraft;

public class ProductMetadataRepo : RootRepo, IProductMetadaRepo
{
    /// <inheritdoc />
    public ProductMetadataRepo(IDbContextFactory<ApplicationContext> dbContextFactory) : base(dbContextFactory)
    {
    }

    public async Task<ProductMetadata> AddAsync(ProductMetadata obj, CancellationToken token = default)
        => await ExecuteAsync(async (context, cancellationToken) =>
        {
            Model.ProductMetadata mpt = new Model.ProductMetadata
            {
                FileKey = obj.FileKey,
                Location = obj.Location,
                OriginalFile = obj.OriginalFile,
                ProductId = obj.ProductId,
                Order = obj.Order
            };

            await context.ProductMetadata.AddAsync(mpt, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            obj.Id = mpt.Id;
            return obj;
        }, token);

    public Task<ProductMetadata?> GetByIdAsync(long id, CancellationToken token = default) =>
        throw new NotImplementedException();

    public Task<List<ProductMetadata>> GetAsync(ProductMetadata obj, ushort searchFlag = 0,
        CancellationToken token = default) => throw new NotImplementedException();

    public Task<ProductMetadata?> UpdateAsync(ProductMetadata obj, CancellationToken token = default) =>
        throw new NotImplementedException();

    public Task DeleteAsync(ProductMetadata obj, CancellationToken token = default) =>
        throw new NotImplementedException();
}