using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using XatiCraft.ApiContracts;
using XatiCraft.Guards;
using XatiCraft.Handlers.Api;

namespace XatiCraft.Controllers;

/// <summary>
/// </summary>
[ApiController]
[Route("product/{productId:long}/storage")]
[EnableRateLimiting("policy_session")]
[IpSessionGuard]
[IpAddressGuard]
[ApiKeyGuard]
public class StorageController : ControllerBase
{
    /// <summary>
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="productId"></param>
    /// <param name="file"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiContract> UploadProductFile(
        [FromServices] IUploadProductFileHandler handler,
        [FromRoute] long productId,
        IFormFile file,
        CancellationToken cancellation)
    {
        await using Stream stream = file.OpenReadStream();
        return await handler.HandleAsync(new UploadProductFileContext(productId, stream, file.FileName), cancellation);
    }
}