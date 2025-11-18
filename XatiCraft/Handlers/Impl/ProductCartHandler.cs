using System.Text;
using XatiCraft.ApiContracts;
using XatiCraft.Data.Repos;
using XatiCraft.Guards;
using XatiCraft.Handlers.Api;
using XatiCraft.Objects;

namespace XatiCraft.Handlers.Impl;

/// <inheritdoc />
public class ProductCartHandler : IProductCartHandler
{
    private const string CookieKey = "_pcc";
    private const char Delimiter = ';';
    private readonly IProductRepo _productRepo;
    private readonly Security _security;

    /// <summary>
    /// </summary>
    public ProductCartHandler(IEnumerable<Security> securities, IProductRepo productRepo)
    {
        _productRepo = productRepo;
        _security = securities.First(s => s is AspDataProtector);
    }

    /// <inheritdoc />
    public ValueTask<ApiContract> HandleAsync(AddProductInCartContext context, CancellationToken cancellationToken)
    {
        context.ExistingCookies.TryGetValue(CookieKey, out string? existingCookie);
        StringBuilder cookieBuilder = new StringBuilder();
        if (!string.IsNullOrEmpty(existingCookie))
        {
            string unpackedCookie = _security.UnPack(existingCookie);
            cookieBuilder.Append(unpackedCookie);
        }

        cookieBuilder.Append(context.ProductId);
        cookieBuilder.Append(Delimiter);

        AddProductInCartContract contract =
            new AddProductInCartContract(true, _security.Pack(cookieBuilder.ToString()), cookieBuilder.ToString(),
                CookieKey);
        return ValueTask.FromResult<ApiContract>(contract);
    }

    /// <inheritdoc />
    public async ValueTask<ApiContract> HandleAsync(GetProductsContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context.Cookies);
        ArgumentNullException.ThrowIfNull(context.FromCookies);

        if (!context.Cookies.TryGetValue(CookieKey, out string? productsCookie))
            return new GetProductsContract([]);

        HashSet<long> products = new HashSet<long>(_security.UnPack(productsCookie)
            .Split(Delimiter, StringSplitOptions.RemoveEmptyEntries).Select(long.Parse));

        List<Product> data = await _productRepo.SelectProductsAsync(cancellationToken);
        return new GetProductsContract(data.Where(d => d.Id.HasValue && products.Contains(d.Id.Value)));
    }
}