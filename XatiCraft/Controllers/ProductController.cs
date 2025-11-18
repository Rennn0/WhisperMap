using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using XatiCraft.ApiContracts;
using XatiCraft.Guards;
using XatiCraft.Handlers.Api;
using XatiCraft.Handlers.Impl;
using XatiCraft.Objects;

namespace XatiCraft.Controllers;

/// <summary>
///     defines operations on product
/// </summary>
[ApiController]
[Route("p")]
[EnableRateLimiting(AuthGuard.SessionPolicy)]
[IpSessionGuard]
[ApiKeyGuard]
public class ProductController : ControllerBase
{
    /// <summary>
    ///     creates new product entry
    /// </summary>
    /// <param name="product"></param>
    /// <param name="handler"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [IpAddressGuard]
    public async Task<ApiContract> CreateProduct([FromServices] ICreateProductHandler handler,
        [FromBody] Product product,
        CancellationToken cancellationToken)
    {
        return await handler.HandleAsync(new CreateProductContext(product.Title, product.Description, product.Price),
            cancellationToken);
    }

    /// <summary>
    /// </summary>
    /// <param name="handlers"></param>
    /// <param name="query"></param>
    /// <param name="fromCookies"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiContract> GetProducts(
        [FromServices] IEnumerable<IHandler<ApiContract, GetProductsContext>> handlers,
        [FromQuery(Name = "q")] string? query,
        [FromQuery(Name = "fc")] bool? fromCookies,
        CancellationToken cancellationToken)
    {
        IHandler<ApiContract, GetProductsContext> handler = (fromCookies ?? false) switch
        {
            true => handlers.First(h => h is ProductCartHandler),
            false => handlers.First(h => h is GetProductsHandler)
        };
        return await handler.HandleAsync(
            new GetProductsContext(query, fromCookies, HttpContext.Request.Cookies.ToDictionary()), cancellationToken);
    }

    /// <summary>
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="handler"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{productId:long}")]
    public async Task<ApiContract> GetProduct([FromServices] IGetProductHandler handler, [FromRoute] long productId,
        CancellationToken cancellationToken)
    {
        return await handler.HandleAsync(new GetProductContext(productId), cancellationToken);
    }

    /// <summary>
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="productId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("{productId:long}")]
    public async Task<ApiContract> AddProductInCart([FromServices] IProductCartHandler handler,
        [FromRoute] long productId, CancellationToken cancellationToken)
    {
        ApiContract contract = await handler.HandleAsync(
            new AddProductInCartContext(productId, HttpContext.Request.Cookies.ToDictionary()), cancellationToken);
        if (contract is AddProductInCartContract { AsCookie: true } cartContract)
            HttpContext.Response.Cookies.Append(cartContract.CookieKey, cartContract.ProtectedCookie, new CookieOptions
            {
                HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict
            });

        return contract;
    }
}