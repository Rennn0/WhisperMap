using System.Text;
using XatiCraft.ApiContracts;
using XatiCraft.Data.Repos;
using XatiCraft.Handlers.Api;
using XatiCraft.Objects;

namespace XatiCraft.Handlers.Impl;

/// <summary>
/// </summary>
public class GetProductsHandler : IGetProductsHandler
{
    private const int MaxLenDesc = 44;
    private const int MaxLenTitle = 32;
    private readonly IProductRepo _productRepo;

    /// <summary>
    /// </summary>
    /// <param name="productRepo"></param>
    public GetProductsHandler(IProductRepo productRepo)
    {
        _productRepo = productRepo;
    }

    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<ApiContract> HandleAsync(GetProductsContext context, CancellationToken cancellationToken)
    {
        List<Product> products = await _productRepo.SelectProductsAsync(cancellationToken);
        GetProductsContract contract = new GetProductsContract(products.Select(p =>
            new Product(EraseIfLarger(p.Title, MaxLenTitle), EraseIfLarger(p.Description, MaxLenDesc), p.Price)
            {
                Id = p.Id,
                PreviewImg = p.ProductMetadata?.FirstOrDefault(pm => !string.IsNullOrEmpty(pm.Location))?.Location
            }
        ))
        {
            RequestId = context.RequestId
        };
        return contract;
    }

    private static string EraseIfLarger(string str, int maxLen)
    {
        if (string.IsNullOrEmpty(str) || str.Length <= maxLen) return str;

        StringBuilder builder = new StringBuilder();
        builder.Append(str[..MaxLenDesc]);
        builder.Append("...");

        return builder.ToString();
    }
}