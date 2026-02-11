using Microsoft.AspNetCore.Mvc;
using XatiCraft.ApiContracts;
using XatiCraft.Data.Objects;
using XatiCraft.Guards;
using XatiCraft.Handlers.Api;
using XatiCraft.Handlers.Impl;

namespace XatiCraft.Controllers;

/// <summary>
///     defines operations on product
/// </summary>
[ApiController]
[Route("p")]
// [EnableRateLimiting(AuthGuard.SessionPolicy)]
// [IpSessionGuard]
// [ApiKeyGuard]
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
            new GetProductsContext(query), cancellationToken);
    }

    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <param name="handler"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ApiContract> GetProduct([FromServices] IGetProductHandler handler, [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        return await handler.HandleAsync(new GetProductContext(id), cancellationToken);
    }

    /// <summary>
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="productId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("{productId:long}/cart")]
    public async Task<ApiContract> AddProductInCart([FromServices] IProductCartHandler handler,
        [FromRoute] long productId, CancellationToken cancellationToken)
    {
        AddProductInCartContext context = new AddProductInCartContext(productId);
        ApiContract contract = await handler.HandleAsync(
            context, cancellationToken);
        if (contract is AddProductInCartContract { AsCookie: true } cartContract)
            HttpContext.Response.Cookies.Append(cartContract.CookieKey, cartContract.ProtectedCookie, new CookieOptions
            {
                HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax
            });

        return new ApiContract(context);
    }

    /// <summary>
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="productId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("{productId:long}/cart")]
    public async Task<ApiContract> RemoveProductFromCart([FromServices] IProductCartHandler handler,
        [FromRoute] long productId, CancellationToken cancellationToken)
    {
        ApiContext context = new RemoveProductFromCartContext(productId);
        ApiContract contract = await handler.HandleAsync(
            (RemoveProductFromCartContext)context, cancellationToken);
        if (contract is AddProductInCartContract { AsCookie: true } cartContract)
            HttpContext.Response.Cookies.Append(cartContract.CookieKey, cartContract.ProtectedCookie, new CookieOptions
            {
                HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict
            });

        return new ApiContract(context);
    }
}