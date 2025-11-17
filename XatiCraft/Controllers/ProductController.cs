using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using XatiCraft.ApiContracts;
using XatiCraft.Guards;
using XatiCraft.Handlers.Api;
using XatiCraft.Objects;

namespace XatiCraft.Controllers;

/// <summary>
///     defines operations on product
/// </summary>
[ApiController]
[Route("p")]
[EnableRateLimiting("policy_session")]
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
    public async Task<ApiContract> CreateProduct([FromBody] Product product,
        [FromServices] ICreateProductHandler handler,
        CancellationToken cancellationToken)
    {
        return await handler.HandleAsync(new CreateProductContext(product.Title, product.Description, product.Price),
            cancellationToken);
    }

    /// <summary>
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiContract> GetProducts([FromServices] IGetProductsHandler handler,
        [FromQuery(Name = "q")] string? query,
        CancellationToken cancellationToken)
    {
        return await handler.HandleAsync(new GetProductsContext(query), cancellationToken);
    }

    /// <summary>
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="handler"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{productId:long}")]
    public async Task<ApiContract> GetProduct([FromRoute] long productId, [FromServices] IGetProductHandler handler,
        CancellationToken cancellationToken)
    {
        return await handler.HandleAsync(new GetProductContext(productId), cancellationToken);
    }
}