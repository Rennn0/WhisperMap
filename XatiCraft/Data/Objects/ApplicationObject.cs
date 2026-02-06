namespace XatiCraft.Data.Objects;

/// <summary>
/// </summary>
public abstract record ApplicationObject
{
    /// <summary>
    /// </summary>
    public long? Id { get; set; }

    /// <summary>
    /// </summary>
    public string? ObjId { get; set; }
}