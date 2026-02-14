using XatiCraft.Data.Objects;

namespace XatiCraft.Data.Repos;

/// <summary>
/// </summary>
public interface IAuthorizationRepo
{
    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<AuthorizationInfo?> SelectAuthorizationInfoAsync(long id, CancellationToken cancellationToken);

    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<AuthorizationInfo?> SelectAuthorizationInfoAsync(string id, CancellationToken cancellationToken);

    /// <summary>
    /// </summary>
    /// <param name="authorizationInfo"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<AuthorizationInfo> UpsertAuthorizationInfoAsync(AuthorizationInfo authorizationInfo,
        CancellationToken cancellationToken);
}