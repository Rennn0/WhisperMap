using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using XatiCraft.ApiContracts;
using XatiCraft.Guards;
using XatiCraft.Handlers.Api;
using XatiCraft.Handlers.Impl;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;

namespace XatiCraft.Controllers;

/// <summary>
///     defines operations on product
/// </summary>
[ApiController]
[Route("p")]
[EnableRateLimiting(AuthGuard.SessionPolicy)]
[IpSessionGuard]
[ApiKeyGuard]
public class ProductController : ApplicationController
{
    /// <summary>
    ///     creates new product entry
    /// </summary>
    /// <param name="product"></param>
    /// <param name="manager"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [IpAddressGuard]
    public async Task<ApiContract> CreateProduct(
        [FromServices] IProductManager manager,
        [FromBody] Product product,
        CancellationToken cancellationToken
    ) =>
        await manager.HandleAsync(
            new CreateProductContext(product.Title, product.Description, product.Price)
            {
                UserId = UserIdC,
            },
            cancellationToken
        );

    /// <summary>
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="product"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:long}")]
    [IpAddressGuard]
    public async Task<ApiContract> UpdateProduct(
        [FromRoute] long id,
        [FromServices] IProductManager manager,
        [FromBody] Product product,
        CancellationToken cancellationToken
    ) =>
        await manager.HandleAsync(
            new UpdateProductContext(id, product.Title, product.Description, product.Price)
            {
                UserId = UserIdC,
            },
            cancellationToken
        );

    /// <summary>
    /// </summary>
    /// <param name="handlers"></param>
    /// <param name="query"></param>
    /// <param name="orderBy"></param>
    /// <param name="batch"></param>
    /// <param name="continuationToken"></param>
    /// <param name="fromCookies"></param>
    /// <param name="fromCart"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    // [ResponseCache(Location = ResponseCacheLocation.Client,VaryByQueryKeys =
    // ["q", "o", "b", "ct", "fcs", "fct" ],NoStore = false,Duration = 300)]
    public async Task<ApiContract> GetProducts(
        [FromServices] IEnumerable<IHandler<ApiContract, GetProductsContext>> handlers,
        [FromQuery(Name = "q")] string? query,
        [FromQuery(Name = "o")] OrderBy? orderBy = OrderBy.NewestFirst,
        [FromQuery(Name = "b")] uint? batch = null,
        [FromQuery(Name = "ct")] string? continuationToken = null,
        [FromQuery(Name = "fcs")] bool? fromCookies = false,
        [FromQuery(Name = "fct")] bool? fromCart = false,
        CancellationToken cancellationToken = default
    )
    {
        bool isGuest = string.IsNullOrEmpty(UserIdC);
        IHandler<ApiContract, GetProductsContext> handler = (fromCookies, fromCart, isGuest) switch
        {
            (false, false, _) => handlers.First(h => h is GetProductsGeneralHandler),
            (true, false, false) => handlers.First(h => h is GetProductCartCookieHandler),
            (_, true, false) => handlers.First(h => h is GetProductsCartHandler),
            (_, true, true) => handlers.First(h => h is GetProductCartCookieHandler),
            _ => throw new ArgumentOutOfRangeException(nameof(handlers)),
        };
        return await handler.HandleAsync(
            new GetProductsContext(query, continuationToken, batch, OrderBy: orderBy)
            {
                UserId = UserIdC,
            },
            cancellationToken
        );
    }

    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <param name="handler"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ApiContract> GetProduct(
        [FromServices] IGetProductHandler handler,
        [FromRoute] string id,
        CancellationToken cancellationToken
    ) =>
        await handler.HandleAsync(
            new GetProductContext(id) { UserId = UserIdC },
            cancellationToken
        );

    /// <summary>
    /// </summary>
    /// <param name="handlers"></param>
    /// <param name="productId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("{productId:long}/cart")]
    public async Task<ApiContract> AddProductInCart(
        [FromRoute] long productId,
        [FromServices] IEnumerable<IHandler<ApiContract, AddProductInCartContext>> handlers,
        CancellationToken cancellationToken
    )
    {
        AddProductInCartContext context = new AddProductInCartContext(productId)
        {
            UserId = UserIdC,
        };
        foreach (IHandler<ApiContract, AddProductInCartContext> handler in handlers)
        {
            ApiContract contract = await handler.HandleAsync(context, cancellationToken);
            if (contract is AddProductInCartContract { AsCookie: true } cartContract)
                AppendC(cartContract.CookieKey, cartContract.ProtectedCookie);
        }

        return new ApiContract(context);
    }

    /// <summary>
    /// </summary>
    /// <param name="handlers"></param>
    /// <param name="productId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("{productId:long}/cart")]
    public async Task<ApiContract> RemoveProductFromCart(
        [FromServices] IEnumerable<IHandler<ApiContract, RemoveProductFromCartContext>> handlers,
        [FromRoute] long productId,
        CancellationToken cancellationToken
    )
    {
        ApiContext context = new RemoveProductFromCartContext(productId) { UserId = UserIdC };
        foreach (IHandler<ApiContract, RemoveProductFromCartContext> handler in handlers)
        {
            ApiContract contract = await handler.HandleAsync(
                (RemoveProductFromCartContext)context,
                cancellationToken
            );
            if (contract is AddProductInCartContract { AsCookie: true } cartContract)
                AppendC(cartContract.CookieKey, cartContract.ProtectedCookie);
        }

        return new ApiContract(context);
    }
}
