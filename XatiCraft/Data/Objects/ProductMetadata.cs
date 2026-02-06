namespace XatiCraft.Data.Objects;

/// <summary>
/// </summary>
/// <param name="OriginalFile"></param>
/// <param name="FileKey"></param>
/// <param name="Location"></param>
/// <param name="ProductId"></param>
public record ProductMetadata(
    string OriginalFile,
    string FileKey,
    string Location,
    long ProductId)
{
    /// <summary>
    /// </summary>
    public long? Id { get; set; }

    /// <summary>
    /// </summary>
    public DateTime? Timestamp { get; set; }

    /// <summary>
    /// </summary>
    public Product? Product { get; set; }
}