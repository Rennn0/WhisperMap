using XatiCraft.Objects;

namespace XatiCraft.Data.Repos.EfCoreImpl;

public class ProductMetadataRepo : IProductMetadaRepo
{
    private readonly ApplicationContext _context;

    public ProductMetadataRepo(ApplicationContext context)
    {
        _context = context;
    }

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