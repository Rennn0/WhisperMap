using XatiCraft.ApiContracts;
using XatiCraft.Data.Objects;
using XatiCraft.Data.Repos;
using XatiCraft.Data.Repos.EfCoreImpl;
using XatiCraft.Handlers.Api;

namespace XatiCraft.Handlers.Impl;

/// <summary>
/// </summary>
internal class CreateProductHandler : ICreateProductHandler
{
    private readonly IProductRepo _productRepo;

    /// <summary>
    /// </summary>
    /// <param name="productRepos"></param>
    public CreateProductHandler(IEnumerable<IProductRepo> productRepos)
    {
        _productRepo = productRepos.First(p => p is ProductRepo);
    }

    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<ApiContract> HandleAsync(CreateProductContext context,
        CancellationToken cancellationToken)
    {
        Product product = await _productRepo.InsertAsync(
            new Product(context.Title, context.Description, Normalize(context.Price, 10, 3)),
            cancellationToken);

        ArgumentNullException.ThrowIfNull(product.Id);
        return new CreateProductContract((long)product.Id, context);
    }

    /// <summary>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="precision"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    protected virtual decimal Normalize(decimal value, int precision, int scale)
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