namespace XcLib.Data.ApplicationObjects;

/// <summary>
/// </summary>
public abstract record ApplicationObject
{
    /// <summary>
    /// </summary>
    public long? Id { get; set; }

    /// <summary>
    /// </summary>
    public string? ObjId { get; init; }
}