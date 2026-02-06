namespace XatiCraft.Data.Objects;

/// <summary>
/// </summary>
/// <param name="Title"></param>
/// <param name="Description"></param>
/// <param name="Price"></param>
public record Product(
    string Title,
    string Description,
    decimal Price
)
{
    /// <summary>
    /// </summary>
    public long? Id { get; set; }

    /// <summary>
    /// </summary>
    public ICollection<ProductMetadata>? ProductMetadata { get; init; }

    /// <summary>
    /// </summary>
    public DateTime? Timestamp { get; set; }

    /// <summary>
    /// </summary>
    public string? PreviewImg { get; set; }
}