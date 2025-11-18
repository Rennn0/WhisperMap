using XatiCraft.ApiContracts;

namespace XatiCraft.Handlers.Api;

/// <summary>
/// </summary>
/// <typeparam name="TContract"></typeparam>
/// <typeparam name="TContext"></typeparam>
public interface IHandler<TContract, in TContext>
    where TContract : ApiContract
    where TContext : ApiContext
{
    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<TContract> HandleAsync(TContext context, CancellationToken cancellationToken);
}