using XatiCraft.ApiContracts;
using XatiCraft.Data.Objects;
using XatiCraft.Guards;
using XatiCraft.Handlers.Api;

namespace XatiCraft.Handlers.Impl;

/// <inheritdoc />
internal class ProductCartHandler : IProductCartHandler
{
    private const string CookieKey = "_pcc";
    private const char Delimiter = ';';
    private readonly IGetProductsHandler _getProductsHandler;
    private readonly HttpContext _httpContext;
    private readonly Security _security;

    /// <summary>
    /// </summary>
    public ProductCartHandler(IEnumerable<Security> securities, IGetProductsHandler getProductsHandler,
        IHttpContextAccessor httpContextAccessor)
    {
        _getProductsHandler = getProductsHandler;
        _security = securities.First(s => s is AspDataProtector);
        _httpContext = httpContextAccessor.HttpContext ??
                       throw new NullReferenceException(nameof(httpContextAccessor.HttpContext));
    }

    /// <inheritdoc />
    public ValueTask<ApiContract> HandleAsync(AddProductInCartContext context, CancellationToken cancellationToken)
    {
        string cookie = GetPlainCookie();
        string newCookie = string.Join(Delimiter,
            [..cookie.Split(Delimiter, StringSplitOptions.RemoveEmptyEntries).Distinct(), $"{context.ProductId};"]);
        AddProductInCartContract contract =
            new AddProductInCartContract(true, _security.Pack(newCookie), newCookie,
                CookieKey, context);
        return ValueTask.FromResult<ApiContract>(contract);
    }

    /// <inheritdoc />
    public async ValueTask<ApiContract> HandleAsync(GetProductsContext context, CancellationToken cancellationToken)
    {
        ApiContract contract = await _getProductsHandler.HandleAsync(new GetProductsContext(), cancellationToken);
        return new GetProductsContract(
            ((GetProductsContract)contract).Products.Where(d =>
                d.Id.HasValue && GetProductIdsFromCookies().Any(id => id == d.Id.Value)),
            context);
    }

    /// <inheritdoc />
    public ValueTask<ApiContract> HandleAsync(RemoveProductFromCartContext context, CancellationToken cancellationToken)
    {
        string plainCookie = GetPlainCookie();
        if (string.IsNullOrEmpty(plainCookie)) return ValueTask.FromResult(new ApiContract(context));

        string newCookie = string.Join(Delimiter,
            plainCookie.Split(Delimiter, StringSplitOptions.RemoveEmptyEntries).Where(id =>
                long.TryParse(id, out long lid) &&
                lid != context.ProductId));

        return new ValueTask<ApiContract>(
            new AddProductInCartContract(true, _security.Pack(newCookie), newCookie,
                CookieKey, context));
    }

    /// <summary>
    /// </summary>
    /// <param name="product"></param>
    /// <returns></returns>
    public bool ExistsInCart(Product product)
    {
        return GetProductIdsFromCookies().Any(id => product.Id == id);
    }

    private HashSet<long> GetProductIdsFromCookies()
    {
        return
        [
            ..GetPlainCookie()
                .Split(Delimiter, StringSplitOptions.RemoveEmptyEntries).Select(long.Parse)
        ];
    }

    private string GetPlainCookie()
    {
        Dictionary<string, string> cookies = _httpContext.Request.Cookies.ToDictionary();

        return !cookies.TryGetValue(CookieKey, out string? protectedCookie)
            ? string.Empty
            : _security.UnPack(protectedCookie);
    }
}