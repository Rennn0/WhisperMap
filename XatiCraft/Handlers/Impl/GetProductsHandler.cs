using System.Text;
using XatiCraft.ApiContracts;
using XatiCraft.Data.Objects;
using XatiCraft.Data.Repos;
using XatiCraft.Data.Repos.EfCoreImpl;
using XatiCraft.Handlers.Api;

namespace XatiCraft.Handlers.Impl;

/// <summary>
/// </summary>
internal class GetProductsHandler : IGetProductsHandler
{
    private const int MaxLenDesc = 44;
    private const int MaxLenTitle = 32;
    private readonly IProductRepo _productRepos;

    /// <summary>
    /// </summary>
    /// <param name="productRepos"></param>
    public GetProductsHandler(IEnumerable<IProductRepo> productRepos)
    {
        _productRepos = productRepos.First(p => p is ProductRepo);
    }

    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<ApiContract> HandleAsync(GetProductsContext context, CancellationToken cancellationToken)
    {
        List<Product> products = await _productRepos.SelectProductsAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(context.Query))
            products = products
                .Where(p =>
                        p.Title.Contains(context.Query, StringComparison.InvariantCultureIgnoreCase) ||
                        p.Description.Contains(context.Query, StringComparison.InvariantCultureIgnoreCase) ||
                        (decimal.TryParse(context.Query, out decimal price) &&
                         Math.Abs(p.Price - price) <= 5) //#NOTE can be customized
                )
                .ToList();
        GetProductsContract contract = new GetProductsContract(products.Select(p =>
            new Product(EraseIfLarger(p.Title, MaxLenTitle), EraseIfLarger(p.Description, MaxLenDesc), p.Price)
            {
                Id = p.Id,
                PreviewImg = p.ProductMetadata?.Where(pm => !string.IsNullOrEmpty(pm.Location)).MinBy(pm => pm.Id)
                    ?.Location //#NOTE maybe add order priority?
            }
        ), context);
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