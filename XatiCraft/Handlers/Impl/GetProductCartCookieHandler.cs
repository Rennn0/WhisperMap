using XatiCraft.ApiContracts;
using XatiCraft.Data.Objects;
using XatiCraft.Guards;
using XatiCraft.Handlers.Api;

namespace XatiCraft.Handlers.Impl;

/// <inheritdoc />
internal class GetProductCartCookieHandler : IProductCartHandler
{
    private const string CookieKey = "__xc_pcc";
    private const char Delimiter = ';';
    private readonly IGetProductsHandler _getProductsHandler;
    private readonly HttpContext _httpContext;
    private readonly Security _security;

    /// <summary>
    /// </summary>
    public GetProductCartCookieHandler(IEnumerable<Security> securities, IGetProductsHandler getProductsHandler,
        IHttpContextAccessor httpContextAccessor)
    {
        _getProductsHandler = getProductsHandler;
        _security = securities.First(s => s is SimpleBase64Protector);
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
        ApiContract contract =
            await _getProductsHandler.HandleAsync(new GetProductsContext(Ids: GetProductIdsFromCookies()),
                cancellationToken);
        return new GetProductsContract(((GetProductsContract)contract).Products, context);
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
    public bool ExistsInCart(Product product) => GetProductIdsFromCookies().Any(id => product.Id == id);

    private HashSet<long> GetProductIdsFromCookies() =>
    [
        ..GetPlainCookie()
            .Split(Delimiter, StringSplitOptions.RemoveEmptyEntries).Select(long.Parse)
    ];

    private string GetPlainCookie() =>
        !_httpContext.Request.Cookies.ToDictionary().TryGetValue(CookieKey, out string? protectedCookie)
            ? string.Empty
            : _security.UnPack(protectedCookie);
}