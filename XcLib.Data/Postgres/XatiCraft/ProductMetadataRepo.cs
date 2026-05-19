using Microsoft.EntityFrameworkCore;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;
using XcLib.Data.Postgres.XatiCraft.Context;

namespace XcLib.Data.Postgres.XatiCraft;

/// <inheritdoc />
public class ProductMetadataRepo : IProductMetadaRepo
{
    private readonly IDbContextFactory<ApplicationContext> _dbContextFactory;

    /// <summary>
    ///     implementation using Npgsql.EntityFrameworkCore.PostgreSQL Version=8.0.0
    /// </summary>
    public ProductMetadataRepo(IDbContextFactory<ApplicationContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <inheritdoc />
    public async Task<ProductMetadata> InsertAsync(ProductMetadata productMetadata, CancellationToken cancellationToken)
    {
        await using ApplicationContext context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        Model.ProductMetadata mpt = new Model.ProductMetadata
        {
            FileKey = productMetadata.FileKey,
            Location = productMetadata.Location,
            OriginalFile = productMetadata.OriginalFile,
            ProductId = productMetadata.ProductId,
            Order = productMetadata.Order
        };

        await context.ProductMetadata.AddAsync(mpt, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        productMetadata.Id = mpt.Id;
        productMetadata.Timestamp = mpt.Timestamp;
        return productMetadata;
    }
}