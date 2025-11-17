using XatiCraft.ApiContracts;

namespace XatiCraft.Handlers.Api;

/// <summary>
/// </summary>
public interface IDeleteProductHandler
{
    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<ApiContract> HandleAsync(DeleteProductContext context, CancellationToken cancellationToken);
}