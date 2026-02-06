using XatiCraft.Data.Objects;

namespace XatiCraft.Data.Repos.EfCoreImpl;

/// <inheritdoc />
internal class ProductMetadataRepo : IProductMetadaRepo
{
    private readonly ApplicationContext _context;

    /// <summary>
    ///     implementation using Npgsql.EntityFrameworkCore.PostgreSQL Version=8.0.0
    /// </summary>
    /// <param name="context"></param>
    public ProductMetadataRepo(ApplicationContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<ProductMetadata> InsertAsync(ProductMetadata productMetadata, CancellationToken cancellationToken)
    {
        Model.ProductMetadata mpt = new Model.ProductMetadata
        {
            FileKey = productMetadata.FileKey,
            Location = productMetadata.Location,
            OriginalFile = productMetadata.OriginalFile,
            ProductId = productMetadata.ProductId
        };

        await _context.ProductMetadata.AddAsync(mpt, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        productMetadata.Id = mpt.Id;
        productMetadata.Timestamp = mpt.Timestamp;
        return productMetadata;
    }
}