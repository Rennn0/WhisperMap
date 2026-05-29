using XatiCraft.ApiContracts;
using XatiCraft.Handlers.Api;
using XatiCraft.Handlers.Upload;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;
using XcLib.Data.Postgres.XatiCraft;

namespace XatiCraft.Handlers.Impl;

/// <summary>
/// </summary>
internal class ProductManager : IProductManager
{
    private readonly IUploader _uploader;
    private readonly IProductRepo _productRepo;
    private readonly IProductRepo _productRepoMongo;
    private readonly IProductMetadaRepo _productMetadataRepo;

    public ProductManager(IEnumerable<IProductRepo> iproductRepos, IEnumerable<IProductMetadaRepo> iproductMetadaRepos,
        IUploader uploader)
    {
        _uploader = uploader;
        IEnumerable<IProductRepo> productRepos = iproductRepos.ToList();
        IEnumerable<IProductMetadaRepo> productMetadaRepos = iproductMetadaRepos.ToList();
        _productRepo = productRepos.First(p => p is ProductRepo);
        _productRepoMongo = productRepos.First(p => p is XcLib.Data.Mongo.XatiCraft.ProductRepo);
        _productMetadataRepo = productMetadaRepos.First(p => p is ProductMetadataRepo); 
    }

    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<ApiContract> HandleAsync(CreateProductContext context,
        CancellationToken cancellationToken)
    {
        Product product = await _productRepo.AddAsync(
            new Product(context.Title, context.Description, Normalize(context.Price, 10, 3)),
            cancellationToken);
        product = await _productRepoMongo.AddAsync(product, cancellationToken);
        ArgumentNullException.ThrowIfNull(product.Id);
        return new CreateProductContract(product.Id.Value, context) { ObjId = product.ObjId };
    }

    public async ValueTask<ApiContract> HandleAsync(UpdateProductContext context, CancellationToken cancellationToken)
    {
        await _productRepo.UpdateAsync(new Product(context.Title, context.Description, Normalize(context.Price, 10, 3) ){ Id = context.Id},
            0, cancellationToken);
        List<Task> productMetadataDeketeTasks = [];
        if (context.ResourceIdsTobeDelete is { Count: > 0 } ids)
            foreach (uint resId in ids)
            {
                ProductMetadata meta = await _productMetadataRepo.GetByIdAsync(resId, cancellationToken);
                productMetadataDeketeTasks.Add(_productMetadataRepo.DeleteAsync(new ProductMetadata { Id = resId },
                    token: cancellationToken));
                productMetadataDeketeTasks.Add(_uploader.DeleteObjectAsync(meta.FileKey, cancellationToken));
            }

        await Task.WhenAll(productMetadataDeketeTasks);
        return new CreateProductContract(context.Id, context);
    }
    
    
    /// <summary>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="precision"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    protected static decimal Normalize(decimal value, int precision, int scale)
    {
        decimal maxValue = (decimal)Math.Pow(10, precision - scale) - (decimal)Math.Pow(10, -scale);
        decimal minValue = -maxValue;

        if (value > maxValue)
            value = maxValue;
        else if (value < minValue)
            value = minValue;

        return Math.Round(value, scale, MidpointRounding.AwayFromZero);
    }

}