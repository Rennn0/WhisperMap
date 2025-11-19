// ReSharper disable MemberCanBePrivate.Global

namespace XatiCraft.ApiContracts;

/// <summary>
/// </summary>
public record ApiContract
{
    /// <summary>
    /// </summary>
    protected ApiContract()
    {
        Timestamp = DateTimeOffset.Now;
        RequestId = Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    public ApiContract(ApiContext context)
    {
        Timestamp = context.Timestamp;
        RequestId = context.RequestId;
    }

    /// <summary>
    /// </summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>
    /// </summary>
    public string RequestId { get; }
}