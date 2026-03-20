namespace XatiCraft.ApiContracts;

/// <inheritdoc />
public abstract record ApiContext : ApiContract
{
    /// <summary>
    /// </summary>
    public string? UserId { get; init; }
}